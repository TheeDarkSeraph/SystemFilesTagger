using FileTagDB;
using FileTagEF.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileTagEF.Controllers {
    public partial class TagController {
        #region Tag and untag files
        public void TagFile(int tagId, int fileId) {
            using (context = DBController.GetContext()) {
                Tag? tag = context.Tags.Include(t=>t.TaggedFiles).Where(t => t.Id == tagId).FirstOrDefault();
                if (tag == null)
                    return;
                FilePath? fp = context.FilePaths.Where(f => f.Id == fileId).FirstOrDefault();
                if (fp == null)
                    return;
                tag.TaggedFiles.Add(fp);
                context.SaveChanges();
            }
        }
        public void TagFiles(int tagId, List<int> fileIds) { // files are assumed to be in DB
            using (context = DBController.GetContext()) {
                Tag? tag = context.Tags.Include(t => t.TaggedFiles).Where(t => t.Id == tagId).FirstOrDefault();
                if (tag == null)
                    return;
                List<FilePath> fps = context.FilePaths.Where(f => fileIds.Contains(f.Id)).ToList();
                fps.ForEach(fp => { if (!tag.TaggedFiles.Contains(fp)) tag.TaggedFiles.Add(fp); });
                context.SaveChanges();
            }
            // get all IDs, then untag them
        }
        public void UntagFile(int tagId, int fileId) {
            using (context = DBController.GetContext()) {
                Tag? tag = context.Tags.Include(t => t.TaggedFiles).Where(t => t.Id == tagId).FirstOrDefault();
                if (tag == null)
                    return; 
                foreach (var fp in tag.TaggedFiles)
                    if (fp.Id == fileId)
                        tag.TaggedFiles.Remove(fp);
                context.SaveChanges();
            }
        }
        public void UntagFiles(int tagId, List<int> fileIds) {
            using (context = DBController.GetContext()) {
                Tag? tag = context.Tags.Include(t => t.TaggedFiles).Where(t => t.Id == tagId).FirstOrDefault();
                if (tag == null)
                    return;
                HashSet<int> fileIdsQuick = new(fileIds);
                foreach (var fp in tag.TaggedFiles)
                    if (fileIdsQuick.Contains(fp.Id))
                        tag.TaggedFiles.Remove(fp);
                context.SaveChanges();
            }
        }
        #endregion

        public void RemoveFilesWithoutTags() {
            using (context = DBController.GetContext()) {
                List<FilePath> fps = context.FilePaths.Include(f => f.FileTags).Include(f=>f.InnerFiles)
                    .Where(f => f.FileTags.Count == 0).ToList();
                fps.ForEach(fp => context.Remove(fp));
                context.SaveChanges();
            }
        }

        #region Get tag or file ids for tagging relationships
        public List<int> GetFileTags(string file) {
            using (context = DBController.GetContext()) {
                FilePath? fp = context.FilePaths.Include(f => f.FileTags).Where(f => f.Fullpath == file).FirstOrDefault();
                if (fp == null)
                    return new();
                return fp.FileTags.Select(ft => ft.Id).ToList();
            }
        }
        public List<int> GetFileTags(int fileId) {
            using (context = DBController.GetContext()) {
                FilePath? fp = context.FilePaths.Include(f => f.FileTags).Where(f => f.Id == fileId)
                    .AsNoTracking().FirstOrDefault();
                if (fp == null)
                    return new();
                return fp.FileTags.Select(ft => ft.Id).ToList();
            }
        }
        public List<List<int>> GetFilesTags(List<string> files) {
            using (context = DBController.GetContext()) {
                List<List<int>> filesTags = new List<List<int>>();
                List<FilePath> fps = context.FilePaths.Include(f => f.FileTags).Where(f => files.Contains(f.Fullpath))
                    .AsNoTracking().ToList();
                fps.ForEach(fp => filesTags.Add(fp.FileTags.Select(ft => ft.Id).ToList()));
                return filesTags;
            }
        }
        public List<List<int>> GetFilesTags(List<int> fileIds) {
            using (context = DBController.GetContext()) {
                List<List<int>> filesTags = new List<List<int>>();
                List<FilePath> fps = context.FilePaths.Include(f => f.FileTags).Where(f => fileIds.Contains(f.Id)).AsNoTracking()
                    .ToList();
                fps.ForEach(fp => filesTags.Add(fp.FileTags.Select(ft => ft.Id).ToList()));
                return filesTags;
            }
        }
        public List<(string, int)> GetFilesWithTag(int tagId) {
            using (context = DBController.GetContext()) {
                Tag? tag = context.Tags.Include(t => t.TaggedFiles).Where(t => t.Id == tagId).FirstOrDefault();
                if (tag == null)
                    return new List<(string, int)>();
                return tag.TaggedFiles.Select(f => (f.Fullpath, f.Id)).ToList();
            }
        }
        #endregion

        #region MEGA QUERY
        public List<(string, int)> GetFilesWithTagQuery(string tagQuery, bool ignoreCase) {
            //SQL_Latin1_General_CP1_CI_AS
            // Get the files with tags that match each part, apply intersection, then apply subtraction
            using (context = DBController.GetContext()) {
                if (!ignoreCase)
                    UseCaseSensitiveLike();
                tagQuery = AdjustQuery(tagQuery);
                Utils.LogToOutput(tagQuery);
                string[] conditions = Regex.Split(tagQuery, @"\s+");
                List<IEnumerable<FilePath>> queryResults = new List<IEnumerable<FilePath>>();
                List<IEnumerable<FilePath>> queryNegativeResults = new List<IEnumerable<FilePath>>();
                foreach (string condition in conditions) {
                    if (condition[0] == '-') { // one negative
                        queryNegativeResults.Add(context.Tags.Include(t => t.TaggedFiles)
                            .Where(t => EF.Functions.Like(t.Name, condition.Substring(1), @"\"))
                            .SelectMany(t => t.TaggedFiles).Distinct().AsNoTracking().ToList());
                    } else if (condition.Contains('+')) { // multiple positive
                        string[] orGroup = condition.Split('+');
                        IEnumerable<FilePath> result;
                        result = context.Tags.Include(t => t.TaggedFiles)
                            .Where(t => EF.Functions.Like(t.Name, orGroup[0], @"\"))
                            .SelectMany(t => t.TaggedFiles).Distinct().AsNoTracking().ToList();
                        for(int i = 1; i < orGroup.Length; i++)
                            result = result.Union(context.Tags.Include(t => t.TaggedFiles)
                            .Where(t => EF.Functions.Like( t.Name, orGroup[i], @"\"))
                            .SelectMany(t => t.TaggedFiles).Distinct().AsNoTracking().ToList(), new FilePathComparer());
                        queryResults.Add(result.ToList());
                    } else { // one positive
                        queryResults.Add(context.Tags.Include(t => t.TaggedFiles)
                            .Where(t => EF.Functions.Like(t.Name, condition, @"\"))
                            .SelectMany(t => t.TaggedFiles).Distinct().AsNoTracking().ToList());
                    }
                }
                IEnumerable<FilePath> finalResult = queryResults[0];

                for (int i = 1; i < queryResults.Count; i++)
                    finalResult = finalResult.Intersect(queryResults[i], new FilePathComparer()).ToList();
                foreach (var negRes in queryNegativeResults) 
                    finalResult = finalResult.Except(negRes, new FilePathComparer());
                // this is needed because sometimes the context is shared or stays a while, so the case Like On stays on sometimes...
                if (!ignoreCase)
                    TurnOffCaseSensitiveLike(); 
                return finalResult.Select(f => (f.Fullpath, f.Id)).ToList();
            }
        }

        private string AdjustQuery(string tagQuery) {
            if (tagQuery.Contains("-") && !Regex.IsMatch(tagQuery, @"\s+")) // so only one negative query
                tagQuery = "* " + tagQuery;
            // escape the escape character first, then escape the DB characters
            tagQuery = tagQuery.Replace(@"\", @"\\").Replace("'", "''").Replace("_", @"\_").Replace("%", @"\%")
                .Replace('*', '%').Replace('~', '_');
            return tagQuery;
        }
        #endregion
    }
}
