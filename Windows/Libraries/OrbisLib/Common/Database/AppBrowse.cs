using SQLite;
using System.Data;

namespace OrbisSuite.Common.Database
{
    public class Master
    {
        public string type { get; set; }

        public string name { get; set; }

        public string tbl_name { get; set; }

        public int rootpage { get; set; }

        public string sql { get; set; }
    }

    [Table("tbl_appbrowse")]
    public class AppBrowse
    {
        [PrimaryKey, NotNull]
        public string titleId { get; set; } = "";

        public string contentId { get; set; } = "";

        public string titleName { get; set; } = "";

        public string metaDataPath { get; set; } = "";

        public int visible { get; set; } = 0;

        public int sortPriority { get; set; } = 0;

        public int dispLocation { get; set; } = 0;

        public int canRemove { get; set; } = 0;

        public string category { get; set; } = "";

        public int contentSize { get; set; } = 0;

        public string installDate { get; set; } = "";

        public string uiCategory { get; set; } = "";

        public static bool RenameAppBrowseDB(string dbPath, int UserId)
        {
            var db = new SQLiteConnection(dbPath);

            // List all of the tables in this data base.
            var dbList = db.Query<Master>("SELECT name FROM sqlite_master WHERE type = 'table'");

            // Make sure we got some tables back.
            if(dbList.Count <= 0)
            {
                return false;
            }

            // The name of the table with the user id appended to the end.
            var tableName = $"tbl_appbrowse_{UserId.ToString().PadLeft(10, '0')}";

            // Search for the table we want to rename to not have the userId on the end.
            var appBrowseTable = dbList.Find(e => e.name.Equals(tableName))?.name;

            // Make sure we found a table matching our pattern.
            if(appBrowseTable != null)
            {
                // Make sure we haven't already renamed this table.
                if (appBrowseTable.Equals("tbl_appbrowse"))
                {
                    return true;
                }

                // Rename it so we can use it.
                var result = db.Execute($"ALTER TABLE `{appBrowseTable}` RENAME TO `tbl_appbrowse`") > 0;
                db.Close();
                return result;
            }
            else
            {
                return false;
            }
            
        }

        public static List<AppBrowse> GetAppBrowseList(string dbPath)
        {
            var db = new SQLiteConnection(dbPath);
            var result = db.Table<AppBrowse>().ToList();
            db.Close();
            return result;
        }

        /*public static bool Save(string dbPath)
        {

        }*/
    }
}
