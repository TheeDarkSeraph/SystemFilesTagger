using FileTagDB;
using FileTagEF.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTagEF.Controllers {
    public partial class TagController {

        internal TagDBContext context = null!;
        private void UseCaseSensitiveLike() => context.Database.ExecuteSqlRaw("PRAGMA case_sensitive_like=ON;");
        private void TurnOffCaseSensitiveLike() => context.Database.ExecuteSqlRaw("PRAGMA case_sensitive_like=OFF;");


        #region Create tags
        public int CreateTag(string tagName) {
            using (context = DBController.GetContext()) {
                Tag? tag = context.Tags.Where(b => b.Name == tagName).FirstOrDefault();
                if (tag != null)
                    return tag.Id;
                tag = new Tag { Name = tagName };
                context.Tags.Add(tag);
                context.SaveChanges();
                return tag.Id;
            }
        }
        public int CreateTags(List<string> tagNames) {
            using (context = DBController.GetContext()) {
                Dictionary<string, Tag> tagModel= new Dictionary<string, Tag>();
                List<Tag> existingTags = context.Tags.Where(b => tagNames.Contains(b.Name)).ToList();
                foreach(Tag tag in existingTags)
                    tagModel.Add(tag.Name, tag);
                foreach (string tagName in tagNames) {
                    if (!tagModel.ContainsKey(tagName)) {
                        Tag tag = new Tag { Name = tagName };
                        context.Tags.Add(tag);
                        tagModel.Add(tag.Name, tag);
                    }
                }
                context.SaveChanges(); // apply all adds and get ids
                // if we want to retrieve the ids
                //List<int> tagIds = new List<int>();
                //tagNames.ForEach(tagName => tagIds.Add(tagModel[tagName].Id));
                return tagNames.Count - existingTags.Count; // added count
            }
        }
        #endregion

        #region Get Tag or their info
        public int CountTags() {
            using (context = DBController.GetContext())
                return context.Tags.AsNoTracking().Count();
        }
        public string GetTagName(int tagID) {
            using (context = DBController.GetContext()) {
                Tag? tag = context.Tags.Where(t => t.Id == tagID).AsNoTracking().FirstOrDefault();
                return tag == null ? "+NotFound" : tag.Name;
            }
        }
        // won't be used, tags will be kept in memory prolly for faster processing of user requests and suggestions, 1 mil of average 7 chars is 11MB roughly which is still nothing
        // there isn't even 1 mil words...
        public int GetTagID(string tagName) {
            using (context = DBController.GetContext()) {
                Tag? tag = context.Tags.Where(t => t.Name == tagName).AsNoTracking().FirstOrDefault();
                return tag == null ? -1 : tag.Id;
            }
        }


        public List<TagDto> GetAllTags() {
            using (context = DBController.GetContext()) 
                return context.Tags.AsNoTracking().Select(t=>new TagDto(t)).ToList();
        }
        public List<string> GetAllTagsAsStrings() {
            using (context = DBController.GetContext())
                return context.Tags.AsNoTracking().Select(x=>x.Name).ToList();
        }
        #endregion

        #region Rename Tag
        public int RenameTag(int tagId, string newName) {
            using (context = DBController.GetContext()) {
                Tag? tag = context.Tags.Where(t => t.Id == tagId).FirstOrDefault();
                if (tag == null) 
                    return -1;
                if (tag.Name == newName)
                    return tag.Id;
                Tag? tag2 = context.Tags.Where(t => t.Name == newName).FirstOrDefault();
                if (tag2 != null) // name already used
                    return -1;
                tag.Name = newName;
                context.SaveChanges();
                return tag.Id;
            }
        }
        public int RenameTag(string tagName, string newName) { // won't be used... 
            if (tagName == newName)
                return 0; // otherwise 1+ will be returned
            using (context = DBController.GetContext()) {
                Tag? tag = context.Tags.Where(t => t.Name == tagName).FirstOrDefault();
                if (tag == null) // does not exist
                    return -1;
                Tag? tag2 = context.Tags.Where(t => t.Name == newName).FirstOrDefault();
                if (tag2 != null) // name already used
                    return -1;
                tag.Name = newName;
                context.SaveChanges();
                return tag.Id;
            }
        }
        #endregion

        #region Delete Tag
        public int DeleteTag(int tagID) {
            using (context = DBController.GetContext()) {
                Tag? tag = context.Tags.Where(t => t.Id == tagID).FirstOrDefault();
                if (tag == null) 
                    return 0;
                context.Tags.Remove(tag);
                context.SaveChanges();
                return 1;
            }
        }
        #endregion
    }
}
