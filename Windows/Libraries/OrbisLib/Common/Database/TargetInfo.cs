using OrbisSuite.Common.Database.Types;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OrbisSuite.Common.Database
{
    /// <summary>
    /// Information about the targets saved.
    /// </summary>
    [Table("Targets")]
    public class TargetInfo
    {
        [PrimaryKey, AutoIncrement, NotNull]
        public int Id { get; set; }

        /// <summary>
        /// Weather or not this is our default target to be selected on start up.
        /// </summary>
        [NotNull]
        [Column("DefaultTarget")]
        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// The name given to the target.
        /// </summary>
        [NotNull, Unique]
        [Column("TargetName")]
        public string Name { get; set; } = "-";

        /// <summary>
        /// The IP Address as a string.
        /// </summary>
        [NotNull, Unique]
        public string IPAddress { get; set; } = "-";

        /// <summary>
        /// The port used to send payloads to the saved IP Address.
        /// </summary>
        [NotNull]
        public int PayloadPort { get; set; } = 9020;

        private TargetDetails _details;
        public TargetDetails Details
        {
            get
            {
                if( _details == null )
                {
                    var db = new SQLiteConnection(Config.DataBasePath);

                    // Create the table if it doesn't exist already.
                    db.CreateTable<TargetDetails>();

                    _details = db.Find<TargetDetails>(x => x.TargetId == Id);
                    if (_details == null)
                    {
                        _details = new TargetDetails();
                        _details.TargetId = Id;
                        db.Insert(_details);
                        db.Close();
                    }
                    else
                    {
                        db.Close();
                    }
                }

                return _details;
            }
        }

        /// <summary>
        /// Remove the default tag from the other row.
        /// </summary>
        private void CheckDefault()
        {
            var defaultTarget = FindDefaultTarget();
            if (IsDefault && defaultTarget != null && defaultTarget.Id != Id)
            {
                defaultTarget.IsDefault = false;
                defaultTarget.Save();
            }
        }

        /// <summary>
        /// Saves the current information about the target to the database.
        /// </summary>
        /// <returns>Returns true if any rows were effected.</returns>
        public bool Save()
        {
            if (Name == string.Empty || Name == "-")
                return false;

            if (IPAddress == string.Empty || IPAddress == "-")
                return false;

            CheckDefault();

            var db = new SQLiteConnection(Config.DataBasePath);

            // Create the table if it doesn't exist already.
            db.CreateTable<TargetInfo>();

            var result = db.Update(this);
            db.Close();
            return (result > 0);
        }

        public TargetInfo Clone()
        {
            return (TargetInfo)this.MemberwiseClone();
        }

        /// <summary>
        /// Adds a this Target to the data base.
        /// </summary>
        /// <returns>Returns true if a row was added to the database.</returns>
        public bool Add()
        {
            try
            {
                if (Name == string.Empty || Name == "-")
                    return false;

                if (IPAddress == string.Empty || IPAddress == "-")
                    return false;

                CheckDefault();

                var db = new SQLiteConnection(Config.DataBasePath);

                // Create the table if it doesn't exist already.
                db.CreateTable<TargetInfo>();

                var result = db.Insert(this);
                db.Close();
                return (result > 0);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Removes this current Target from the database.
        /// </summary>
        /// <returns>Returns true if a row was removed from the database.</returns>
        public bool Remove()
        {
            var db = new SQLiteConnection(Config.DataBasePath);
            var result = db.Delete(this);
            db.Close();
            return (result > 0);
        }

        public static List<TargetInfo> GetTargetList()
        {
            var db = new SQLiteConnection(Config.DataBasePath);

            // Create the table if it doesn't exist already.
            db.CreateTable<TargetInfo>();

            var result = db.Table<TargetInfo>().ToList();
            db.Close();
            return result;
        }

        public static TargetInfo FindDefaultTarget()
        {
            var db = new SQLiteConnection(Config.DataBasePath);

            // Create the table if it doesn't exist already.
            db.CreateTable<TargetInfo>();

            var result = db.Find<TargetInfo>(x => x.IsDefault == true);
            db.Close();
            return result;
        }

        /// <summary>
        /// Find a saved Target by a specific value using a predicate.
        /// </summary>
        /// <param name="predicate">The predicate of the columns we want to match on.</param>
        /// <returns>Returns the first object that matches the predicate.</returns>
        public static TargetInfo FindTarget(Expression<Func<TargetInfo, bool>> predicate)
        {
            var db = new SQLiteConnection(Config.DataBasePath);

            // Create the table if it doesn't exist already.
            db.CreateTable<TargetInfo>();

            var result = db.Find(predicate);
            db.Close();
            return result;
        }

        /// <summary>
        /// Find weahter or not a Target by specific value exists by using a predicate.
        /// </summary>
        /// <param name="predicate">The predicate of the columns we want to match on.</param>
        /// <returns>Returns true if we found a match.</returns>
        public static bool DoesTargetExist(Expression<Func<TargetInfo, bool>> predicate)
        {
            var db = new SQLiteConnection(Config.DataBasePath);

            // Create the table if it doesn't exist already.
            db.CreateTable<TargetInfo>();

            var result = db.Find(predicate);
            db.Close();
            return (result != null);
        }
    }
}
