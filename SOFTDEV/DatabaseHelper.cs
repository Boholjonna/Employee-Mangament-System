using MySql.Data.MySqlClient;
using System;

namespace SOFTDEV
{
    /// <summary>
    /// Provides MySQL database connectivity using XAMPP (localhost).
    /// Database : userrole
    /// Table    : admin  (columns: id, username, email, password, role)
    /// </summary>
    public static class DatabaseHelper
    {
        // ── Connection string ─────────────────────────────────────────────────
        // XAMPP defaults: host=localhost, port=3306, user=root, no password.
        // Change Password="" if you set a root password in phpMyAdmin.
        private const string ConnectionString =
            "Server=localhost;" +
            "Port=3306;" +
            "Database=userrole;" +
            "Uid=root;" +
            "Pwd=;";          // ← leave empty for default XAMPP root (no password)

        // ── Open connection ───────────────────────────────────────────────────

        /// <summary>Opens and returns a new <see cref="MySqlConnection"/>.</summary>
        public static MySqlConnection GetConnection()
        {
            var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        // ── Admin authentication ──────────────────────────────────────────────

        /// <summary>
        /// Checks whether the supplied credentials match a row in the <c>admin</c> table.
        /// </summary>
        /// <param name="username">The username entered on the login screen.</param>
        /// <param name="password">The plain-text password entered on the login screen.</param>
        /// <returns>
        /// <see langword="true"/> if a matching admin record is found;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public static bool AuthenticateAdmin(string username, string password)
        {
            try
            {
                using var conn = GetConnection();

                // Table columns: id (int 11), username (varchar 255), password (varchar 255)
                const string sql =
                    "SELECT COUNT(*) FROM admin " +
                    "WHERE username = @username AND password = @password";

                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);

                long count = (long)(cmd.ExecuteScalar() ?? 0L);
                return count > 0;
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB] AuthenticateAdmin error: {ex.Message}");
                return false;
            }
        }

        // ── Employee authentication ───────────────────────────────────────────

        /// <summary>
        /// Checks whether the supplied credentials match a row in the <c>employees</c> table.
        /// Add / rename the table name below to match your actual schema.
        /// </summary>
        public static bool AuthenticateEmployee(string username, string password)
        {
            try
            {
                using var conn = GetConnection();

                const string sql =
                    "SELECT COUNT(*) FROM employee " +
                    "WHERE username = @username AND password = @password";

                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);

                long count = (long)(cmd.ExecuteScalar() ?? 0L);
                return count > 0;
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB] AuthenticateEmployee error: {ex.Message}");
                return false;
            }
        }

        // ── Connection test ───────────────────────────────────────────────────

        /// <summary>
        /// Returns <see langword="true"/> if a connection to the database can be opened.
        /// Useful for a startup health-check.
        /// </summary>
        public static bool TestConnection()
        {
            try
            {
                using var conn = GetConnection();
                return conn.State == System.Data.ConnectionState.Open;
            }
            catch
            {
                return false;
            }
        }
    }
}
