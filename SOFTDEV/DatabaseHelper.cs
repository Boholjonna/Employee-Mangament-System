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

        // ── Get admin username ────────────────────────────────────────────────

        /// <summary>
        /// Returns the username from the <c>admin</c> table that matches the supplied credentials.
        /// </summary>
        /// <param name="username">The username entered on the login screen.</param>
        /// <param name="password">The plain-text password entered on the login screen.</param>
        /// <returns>
        /// The matching <c>username</c> string, or <see langword="null"/> if not found.
        /// </returns>
        public static string? GetAdminUsername(string username, string password)
        {
            try
            {
                using var conn = GetConnection();

                const string sql =
                    "SELECT username FROM admin " +
                    "WHERE username = @username AND password = @password " +
                    "LIMIT 1";

                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);

                return cmd.ExecuteScalar() as string;
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB] GetAdminUsername error: {ex.Message}");
                return null;
            }
        }

        // ── Get admin name ────────────────────────────────────────────────────

        /// <summary>
        /// Returns the <c>name</c> column from the <c>admin</c> table that matches the supplied credentials.
        /// </summary>
        /// <param name="username">The username entered on the login screen.</param>
        /// <param name="password">The plain-text password entered on the login screen.</param>
        /// <returns>
        /// The matching <c>name</c> string, or <see langword="null"/> if not found.
        /// </returns>
        public static string? GetAdminName(string username, string password)
        {
            try
            {
                using var conn = GetConnection();

                const string sql =
                    "SELECT name FROM admin " +
                    "WHERE username = @username AND password = @password " +
                    "LIMIT 1";

                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);

                return cmd.ExecuteScalar() as string;
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB] GetAdminName error: {ex.Message}");
                return null;
            }
        }

        // ── Get employee count ────────────────────────────────────────────────

        /// <summary>
        /// Returns the total number of rows in the <c>employee</c> table.
        /// Returns -1 if the query fails (e.g. DB unavailable).
        /// </summary>
        public static int GetEmployeeCount()
        {
            try
            {
                using var conn = GetConnection();
                const string sql = "SELECT COUNT(*) FROM employee";
                using var cmd = new MySqlCommand(sql, conn);
                long count = (long)(cmd.ExecuteScalar() ?? 0L);
                return (int)count;
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB] GetEmployeeCount error: {ex.Message}");
                return -1;
            }
        }

        // ── Get all employees ─────────────────────────────────────────────────

        /// <summary>
        /// Returns all rows from the <c>employee</c> table as a list of
        /// <see cref="EmployeeEntry"/> objects (name + position).
        /// </summary>
        public static List<EmployeeEntry> GetAllEmployees()
        {
            var list = new List<EmployeeEntry>();
            try
            {
                using var conn = GetConnection();

                const string sql = "SELECT name, position FROM employee ORDER BY name";

                using var cmd = new MySqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string name     = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    string position = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    list.Add(new EmployeeEntry(name, position));
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB] GetAllEmployees error: {ex.Message}");
            }
            return list;
        }

        // ── Get employees with financials ─────────────────────────────────────

        /// <summary>
        /// Returns all employees including their salary and payroll figures.
        /// </summary>
        public static List<EmployeeFinancialInfo> GetAllEmployeesWithFinancials()
        {
            var list = new List<EmployeeFinancialInfo>();
            try
            {
                using var conn = GetConnection();

                const string sql =
                    "SELECT id, name, position, salary, payroll " +
                    "FROM employee ORDER BY name";

                using var cmd    = new MySqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new EmployeeFinancialInfo
                    {
                        Id       = reader.IsDBNull(0) ? 0      : reader.GetInt32(0),
                        Name     = reader.IsDBNull(1) ? ""     : reader.GetString(1),
                        Position = reader.IsDBNull(2) ? ""     : reader.GetString(2),
                        Salary   = reader.IsDBNull(3) ? 0m     : Convert.ToDecimal(reader.GetValue(3)),
                        Payroll  = reader.IsDBNull(4) ? 0m     : Convert.ToDecimal(reader.GetValue(4)),
                    });
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB] GetAllEmployeesWithFinancials error: {ex.Message}");
            }
            return list;
        }

        // ── Employee authentication ───────────────────────────────────────────

        /// <summary>
        /// Checks whether the supplied credentials match a row in the <c>employee</c> table.
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

        /// <summary>
        /// Returns the <c>name</c> column for the employee matching the supplied credentials.
        /// Falls back to the username if no name is found.
        /// </summary>
        public static string GetEmployeeName(string username, string password)
        {
            try
            {
                using var conn = GetConnection();

                const string sql =
                    "SELECT name FROM employee " +
                    "WHERE username = @username AND password = @password " +
                    "LIMIT 1";

                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);

                return cmd.ExecuteScalar() as string ?? username;
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB] GetEmployeeName error: {ex.Message}");
                return username; // fall back to username if DB fails
            }
        }

        // ── Get employee details ──────────────────────────────────────────────

        /// <summary>
        /// Returns all profile fields for the employee with the given name,
        /// or null if no match is found or a database error occurs.
        /// </summary>
        /// <param name="name">The employee's name to look up.</param>
        /// <returns>
        /// An <see cref="EmployeeDetail"/> populated with all profile fields,
        /// or <see langword="null"/> if no matching row exists or a database error occurs.
        /// </returns>
        public static EmployeeDetail? GetEmployeeDetails(string name)
        {
            try
            {
                using var conn = GetConnection();

                const string sql =
                    "SELECT name, position, salary, payroll, datehired, " +
                    "contactno, address, emergencycontact " +
                    "FROM employee " +
                    "WHERE name = @name " +
                    "LIMIT 1";

                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", name);

                using var reader = cmd.ExecuteReader();

                if (!reader.Read())
                    return null;

                return new EmployeeDetail
                {
                    Name             = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                    Position         = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Salary           = reader.IsDBNull(2) ? 0m           : Convert.ToDecimal(reader.GetValue(2)),
                    Payroll          = reader.IsDBNull(3) ? 0m           : Convert.ToDecimal(reader.GetValue(3)),
                    DateHired        = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    ContactNo        = reader.IsDBNull(5) ? string.Empty : Convert.ToString(reader.GetValue(5)) ?? string.Empty,
                    Address          = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                    EmergencyContact = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB] GetEmployeeDetails error: {ex.Message}");
                return null;
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

        // ── Attendance records ────────────────────────────────────────────────

        /// <summary>
        /// Resolves the hex background color for a StatusBadge based on the attendance status string.
        /// </summary>
        /// <param name="status">The raw status string from the database (case-insensitive).</param>
        /// <returns>
        /// A hex color string: Present → #2ecc71, Late → #f39c12, Absent → #e74c3c,
        /// On Leave → #3498db, or #7b61ff for any unrecognised value.
        /// </returns>
        private static string ResolveStatusColor(string status) => status.ToLower() switch
        {
            "present"  => "#2ecc71",
            "late"     => "#f39c12",
            "absent"   => "#e74c3c",
            "on leave" => "#3498db",
            _          => "#7b61ff",
        };

        /// <summary>
        /// Returns all attendance rows for a specific employee, ordered by date descending.
        /// </summary>
        public static List<AttendanceRecord_Model> GetAttendanceByEmployee(string employeeName)
        {
            var list = new List<AttendanceRecord_Model>();
            try
            {
                using var conn = GetConnection();

                const string sql =
                    "SELECT employeename, `date`, timein, timeout, totalhours " +
                    "FROM attendance " +
                    "WHERE employeename = @name " +
                    "ORDER BY `date` DESC";

                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", employeeName);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string timeIn = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    string status = DetermineStatus(timeIn);
                    list.Add(new AttendanceRecord_Model
                    {
                        EmployeeName = reader.IsDBNull(0) ? "" : reader.GetString(0),
                        Date         = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        TimeIn       = timeIn,
                        TimeOut      = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        TotalHours   = reader.IsDBNull(4) ? "" : reader.GetString(4),
                        Status       = status,
                        StatusColor  = ResolveStatusColor(status),
                    });
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB] GetAttendanceByEmployee error: {ex.Message}");
            }
            return list;
        }

        /// <summary>
        /// Returns all rows from the <c>attendance</c> table,
        /// ordered by date descending then employee name ascending.
        /// Returns an empty list if a <see cref="MySqlException"/> occurs.
        /// </summary>
        public static List<AttendanceRecord_Model> GetAllAttendanceRecords()
        {
            var list = new List<AttendanceRecord_Model>();
            try
            {
                using var conn = GetConnection();

                const string sql =
                    "SELECT employeename, `date`, timein, timeout, totalhours " +
                    "FROM attendance " +
                    "ORDER BY `date` DESC, employeename ASC";

                using var cmd    = new MySqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string timeIn  = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    string status  = DetermineStatus(timeIn);
                    list.Add(new AttendanceRecord_Model
                    {
                        EmployeeName = reader.IsDBNull(0) ? "" : reader.GetString(0),
                        Date         = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        TimeIn       = timeIn,
                        TimeOut      = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        TotalHours   = reader.IsDBNull(4) ? "" : reader.GetString(4),
                        Status       = status,
                        StatusColor  = ResolveStatusColor(status),
                    });
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB] GetAllAttendanceRecords error: {ex.Message}");
            }
            return list;
        }

        /// <summary>
        /// Records a clock-in event for an employee.
        /// If a record for today already exists, updates the time-out and total hours instead.
        /// </summary>
        /// <param name="employeeName">The employee's name as stored in the attendance table.</param>
        /// <param name="time">The clock-in or clock-out time string (e.g. "08:30 AM").</param>
        /// <param name="isClockinIn"><see langword="true"/> to record time-in; <see langword="false"/> to record time-out.</param>
        /// <returns><see langword="true"/> if the operation succeeded.</returns>
        public static bool RecordAttendance(string employeeName, string time, bool isClockingIn)
        {
            try
            {
                using var conn = GetConnection();
                string today = DateTime.Today.ToString("yyyy-MM-dd");

                // Check if a record already exists for this employee today
                // `date` is backtick-escaped because it is a MySQL reserved word
                const string checkSql =
                    "SELECT timein FROM attendance " +
                    "WHERE employeename = @name AND `date` = @date " +
                    "LIMIT 1";

                using var checkCmd = new MySqlCommand(checkSql, conn);
                checkCmd.Parameters.AddWithValue("@name", employeeName);
                checkCmd.Parameters.AddWithValue("@date", today);
                var existingTimeIn = checkCmd.ExecuteScalar() as string;

                if (isClockingIn)
                {
                    if (existingTimeIn != null)
                    {
                        // Already clocked in today — update time-in
                        const string updateSql =
                            "UPDATE attendance SET timein = @time " +
                            "WHERE employeename = @name AND `date` = @date";
                        using var updateCmd = new MySqlCommand(updateSql, conn);
                        updateCmd.Parameters.AddWithValue("@time", time);
                        updateCmd.Parameters.AddWithValue("@name", employeeName);
                        updateCmd.Parameters.AddWithValue("@date", today);
                        return updateCmd.ExecuteNonQuery() > 0;
                    }
                    else
                    {
                        // New record for today
                        const string insertSql =
                            "INSERT INTO attendance (employeename, `date`, timein) " +
                            "VALUES (@name, @date, @time)";
                        using var insertCmd = new MySqlCommand(insertSql, conn);
                        insertCmd.Parameters.AddWithValue("@name", employeeName);
                        insertCmd.Parameters.AddWithValue("@date", today);
                        insertCmd.Parameters.AddWithValue("@time", time);
                        return insertCmd.ExecuteNonQuery() > 0;
                    }
                }
                else
                {
                    // Clock-out: update timeout and calculate total hours
                    string totalHours = CalculateTotalHours(existingTimeIn ?? "", time);
                    const string updateSql =
                        "UPDATE attendance SET timeout = @time, totalhours = @total " +
                        "WHERE employeename = @name AND `date` = @date";
                    using var updateCmd = new MySqlCommand(updateSql, conn);
                    updateCmd.Parameters.AddWithValue("@time", time);
                    updateCmd.Parameters.AddWithValue("@total", totalHours);
                    updateCmd.Parameters.AddWithValue("@name", employeeName);
                    updateCmd.Parameters.AddWithValue("@date", today);
                    return updateCmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB] RecordAttendance error: {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }

        /// <summary>Calculates total hours between two time strings.</summary>
        private static string CalculateTotalHours(string timeIn, string timeOut)
        {
            if (string.IsNullOrEmpty(timeIn) || string.IsNullOrEmpty(timeOut))
                return "";

            if (DateTime.TryParse(timeIn, out var inTime) &&
                DateTime.TryParse(timeOut, out var outTime))
            {
                var duration = outTime - inTime;
                if (duration.TotalHours > 0)
                    return duration.TotalHours.ToString("F1");
            }
            return "";
        }

        /// <summary>
        /// Determines attendance status from the clock-in time.
        /// On time (≤ 08:00) → Present; after 08:00 → Late; empty → Absent.
        /// </summary>
        private static string DetermineStatus(string timeIn)
        {
            if (string.IsNullOrEmpty(timeIn))
                return "Absent";

            if (DateTime.TryParse(timeIn, out var clockIn))
                return clockIn.TimeOfDay <= new TimeSpan(8, 0, 0) ? "Present" : "Late";

            return "Present";
        }
    }
}
