using System;
using Microsoft.Data.SqlClient;

class Program {
    static void Main() {
        string[] conns = {
            "Server=.\\SQLEXPRESS;Database=ENSOPSİKOLOJİ;Trusted_Connection=True;TrustServerCertificate=True;",
            "Server=localhost\\SQLEXPRESS;Database=ENSOPSİKOLOJİ;Trusted_Connection=True;TrustServerCertificate=True;",
            "Server=.;Database=ENSOPSİKOLOJİ;Trusted_Connection=True;TrustServerCertificate=True;",
            "Server=(localdb)\\mssqllocaldb;Database=ENSOPSİKOLOJİ;Trusted_Connection=True;TrustServerCertificate=True;"
        };

        foreach (var connStr in conns) {
            try {
                using var conn = new SqlConnection(connStr);
                conn.Open();
                Console.WriteLine("SUCCESS: " + connStr);
                using var cmd = new SqlCommand("SELECT COUNT(*) FROM Hastalar", conn);
                Console.WriteLine("Hastalar Count: " + cmd.ExecuteScalar());
                return;
            } catch (Exception ex) {
                Console.WriteLine("FAILED: " + connStr + " -> " + ex.Message);
            }
        }
    }
}
