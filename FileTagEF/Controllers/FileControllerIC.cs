using FileTagDB;
using FileTagEF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTagEF.Controllers {
    // IC is in context, as in in EF DB context being used
    public partial class FileController {

        #region Adding multiple files
        //internal void AddNode(int fileID, FileNode node) {
        //    // We want to add ALL the files with the one insert
        //    AddFileChilds(GetFilePathIC(fileID), fileID); // in case previous children exists in the database to this file
        //    if (node.children.Count == 0)
        //        return;

        //    if (node.children.Count == 1) { // forgot to add the child...
        //        int resultID = AddFileIC(node.children[0].Path);
        //        if (-1 == resultID)
        //            Utils.LogToOutput("Possible problem in bulk add Mini node path: " + node.Path);
        //        AddNode(resultID, node.children[0]);
        //        return;
        //    }
        //    MultiNodeInsert(node.children, fileID);
        //    //Utils.LogToOutput($" start i/total = {i} / {node.children.Count} Move {Math.Min(bulkSeparation, node.children.Count - i)} // par: {node.Path}");
        //    // but now we need to insert my and child ID for all?
        //    foreach (FileNode fn in node.children) {
        //        int nodeID = GetFileIdIC(fn.Path);
        //        if (-1 == nodeID)
        //            Utils.LogToOutput("Possible problem in bulk add Mini node path2: " + node.Path);
        //        AddNode(nodeID, fn);
        //    }
        //}
        //internal void MultiNodeInsert(List<FileNode> children, int parentID, bool hasParent = true) {
        //    List<int> childFileIds = new();
        //    foreach (FileNode childNode in children)
        //        childFileIds.Add(AddFileIC(childNode.Path, false)); // takes care of parent and child (if child exists)
        //    context.SaveChanges();
        //    if (hasParent)
        //        AddParentChildRelations(parentID, new HashSet<int>(childFileIds));
        //    context.SaveChanges();
        //}


        #endregion

        #region Adding multiple files, bulk EF support

        internal FilePath? AddAndGetFileIC(string filepath, bool addRelationConnections = true, bool shouldSave = true) {
            filepath = FixFilePath(filepath);
            if (GetParentPath(filepath) == "")
                return null;
            FilePath? file = GetFilePathIC(filepath);
            if (file != null)
                return file;
            FilePath fp = new FilePath { Fullpath = filepath, InnerFiles = new HashSet<FilePath>() };
            context.Add(fp);
            if (addRelationConnections) AddFileConnections(fp);
            if (shouldSave) context.SaveChanges();
            return fp;
        }

        internal void AddFileConnections(FilePath fp) {
            AddFileParent(fp);
            AddFileChilds(fp);
        }
        internal void AddFileParent(FilePath fp) {
            string? parentPath = GetParentPath(fp.Fullpath);
            if (parentPath == null)
                return;
            FilePath? parentFile = GetFilePathIC(parentPath);
            if (parentFile != null)
                AddParentChildRelation(parentFile, fp);
        }
        internal void AddParentChildRelation(FilePath parent, FilePath child) {
            if (parent.Id == child.Id)
                return;
            parent.InnerFiles.Add(child);
            child.ParentFolder = parent;
            child.ParentFolderId = parent.Id;
        }
        internal void AddParentChildRelations(FilePath parent, List<FilePath> childs) {
            if (parent == null || childs.Count == 0)
                return;
            foreach (FilePath child in childs)
                if (!parent.InnerFiles.Contains(child))
                    parent.InnerFiles.Add(child);
        }

        internal void AddFileChilds(FilePath parent) {
            List<FilePath> childFiles = context.FilePaths.Include(f => f.ParentFolder).Where(x => EF.Functions.Like(x.Fullpath, $"{parent.Fullpath}_%")
            && !EF.Functions.Like(x.Fullpath, $"{parent.Fullpath}_%{Path.PathSeparator}%")).ToList();
            foreach (FilePath fp in childFiles)
                if (!parent.InnerFiles.Contains(fp))
                    parent.InnerFiles.Add(fp);
        }
        internal void AddNodeEF(FilePath fp, FileNode node) {
            // We want to add ALL the files with the one insert
            AddFileChilds(fp); // in case previous children exists in the database to this file
            if (node.children.Count == 0)
                return;
            if (node.children.Count == 1) { // forgot to add the child...
                FilePath? newFile= AddAndGetFileIC(node.children[0].Path, true, false);
                if (newFile == null)
                    Utils.LogToOutput("Possible problem in bulk add Mini node path: " + node.Path);
                AddNodeEF(newFile, node.children[0]);
                return;
            }
            MultiNodeInsert(node.children, fp);
            //Utils.LogToOutput($" start i/total = {i} / {node.children.Count} Move {Math.Min(bulkSeparation, node.children.Count - i)} // par: {node.Path}");
            // but now we need to insert my and child ID for all?
            foreach (FileNode fn in node.children) {
                FilePath? newFile = GetFilePathIC(fn.Path);
                if (newFile == null)
                    Utils.LogToOutput("Possible problem in bulk add Mini node path2: " + node.Path);
                else
                    AddNodeEF(newFile, fn);
            }
        }
        internal void MultiNodeInsert(List<FileNode> children,FilePath parent, bool hasParent = true) {
            List<FilePath> childFilePaths = new();
            foreach (FileNode childNode in children) {
                FilePath? childFp = AddAndGetFileIC(childNode.Path, false, false);
                if(childFp!=null)
                    childFilePaths.Add(childFp); // takes care of parent and child (if child exists)
            }
            if (hasParent)
                AddParentChildRelations(parent, childFilePaths);
            context.SaveChanges(); // this is necessary, otherwise some things break on retrieval in the whole bulk insert process
        }
        #endregion

        #region Adding single file
        internal int AddFileIC(string filepath, bool addRelationConnections = true, bool save = true) {
            filepath = FixFilePath(filepath);
            if (GetParentPath(filepath) == "")
                return -1;
            
            int existingID = GetFileIdIC(filepath);
            if (-1 != existingID)
                return existingID;
            FilePath fp = new FilePath { Fullpath = filepath };
            context.Add(fp);
            if(addRelationConnections)
                AddFileConnections(filepath, fp.Id);
            if(save)
                context.SaveChanges();
            return fp.Id;
        }
        internal void AddFileConnections(string filepath, int fileID) {
            AddFileParent(filepath, fileID);
            AddFileChilds(filepath, fileID);
        }
        internal void AddFileParent(string filepath, int fileID) {
            string? parentPath = GetParentPath(filepath);
            if (parentPath == null)
                return;
            int parentFileID = GetFileIdIC(parentPath);
            if (-1 != parentFileID)
                AddParentChildRelation(parentFileID, fileID);
        }
        internal void AddParentChildRelation(int parentFileID, int childId) {
            if (parentFileID == childId)// can't have child be parent of itself
                return;
            FilePath? parent = context.FilePaths.Include(f => f.InnerFiles).FirstOrDefault(file => file.Id == parentFileID);
            FilePath? child = context.FilePaths.Include(f => f.ParentFolder).FirstOrDefault(file => file.Id == childId);
            if (parent == null || child == null)
                return;
            parent.InnerFiles.Add(child);
            //child.ParentFolder = parent;
            //child.ParentFolderId = parent.Id;
        }
        internal void AddParentChildRelations(int parentFileID, HashSet<int> childId) {
            FilePath? parent = context.FilePaths.Include(f => f.InnerFiles).FirstOrDefault(file => file.Id == parentFileID);
            List<FilePath> childs = context.FilePaths.Where(file => childId.Contains(file.Id)).ToList();
            if (parent == null || childs.Count == 0)
                return;
            foreach (FilePath child in childs)
                if (!parent.InnerFiles.Contains(child))
                    parent.InnerFiles.Add(child);
        }
        internal void AddFileChilds(string parentFilepath, int parentFileID) {
            FilePath? parent = context.FilePaths.Include(f => f.InnerFiles).FirstOrDefault(file => file.Id == parentFileID);
            if (parent == null)
                return;
            List<FilePath> childFiles = context.FilePaths.Include(f => f.ParentFolder).Where(x => EF.Functions.Like(x.Fullpath, $"{parentFilepath}_%")
            && !EF.Functions.Like(x.Fullpath, $"{parentFilepath}_%{Path.PathSeparator}%")).ToList();
            foreach (FilePath fp in childFiles)
                if (!parent.InnerFiles.Contains(fp))
                    parent.InnerFiles.Add(fp);
        }
        #endregion


        #region Getting File Data for bulk

        internal FilePath? GetFilePathIC(string filepath) {
            return context.FilePaths.Include(f => f.InnerFiles).Where(b => b.Fullpath == filepath).FirstOrDefault();
        }
        #endregion

        #region Getting File Data


        internal int GetFileIdIC(string filepath) {
            FilePath? file = context.FilePaths.Where(b => b.Fullpath==filepath).FirstOrDefault();
            return file == null ? -1 : file.Id;
        }

        internal string GetFilePathIC(int fileId) {
            FilePath? file = context.FilePaths.Where(b => b.Id==fileId).FirstOrDefault();
            return file == null ? string.Empty : file.Fullpath;
        }

        internal List<(string, int)> GetFileChildrenIC(int fileId) {
            FilePath? file = context.FilePaths.Include(file => file.InnerFiles)
                .FirstOrDefault(file => file.Id == fileId);
            if (file == null)
                return new();
            else
                return file.InnerFiles.Select(file => (file.Fullpath, file.Id)).ToList();
        }
        internal List<(string, int)> GetFilesWithPathIC(string filepath) {
            return context.FilePaths.Where(file => file.Fullpath.StartsWith(filepath)).ToList()
                .Select(x => (x.Fullpath, x.Id)).ToList();
        }
        #endregion

        #region Updating filepath
        internal void RenameFilePathIC(string oldPath, string newPath) {
            context.FilePaths.ToList().ForEach(fp => fp.Fullpath = fp.Fullpath.Replace(oldPath, newPath));
            context.SaveChanges();
        }
        #endregion

        #region File Deletion
        // Deletes the relation to parent of this file (because file was renamed to new dir)
        internal void DeleteParentLinkIC(int fileID) {
            FilePath? fp = context.FilePaths.Include(f=>f.ParentFolder).FirstOrDefault(f => f.Id == fileID);
            if (fp == null)
                return;
            fp.ParentFolder = null;
            fp.ParentFolderId = null;
        }
        internal void DeleteFileIC(string filepath) {
            filepath = FixFilePath(filepath);
            int fileID = GetFileIdIC(filepath);
            if (-1 == fileID) return;
            FilePath? fp = context.FilePaths.Include(f => f.ParentFolder).Include(f=>f.InnerFiles)
                .Include(f=>f.FileTags).FirstOrDefault(f => f.Fullpath == filepath);
            if (fp == null)
                return;
            context.FilePaths.Remove(fp);
        }

        internal void DeleteDirectoryIC(string filepath) {
            filepath = FixFilePath(filepath);
            int fileID = GetFileIdIC(filepath);
            if (-1 == fileID) return;
            context.FilePaths.RemoveRange( // removing the includes does not really make a difference...?
                context.FilePaths.Include(f => f.ParentFolder).Include(f => f.InnerFiles)
                .Include(f => f.FileTags).Where(f => f.Fullpath.StartsWith(filepath)));
        }

        #endregion
    }
}
