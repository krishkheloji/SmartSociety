using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SocietyManagement.Admin
{
    public partial class Dashboard : System.Web.UI.Page
    {
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadDashboardStatistics();
            }
        }

        private void LoadDashboardStatistics()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    conn.Open();

                    // 1. Total Flats (Units)
                    string queryFlats = "SELECT COUNT(*) FROM Units";
                    SqlCommand cmdFlats = new SqlCommand(queryFlats, conn);
                    lblTotalFlats.Text = cmdFlats.ExecuteScalar().ToString();

                    // 2. Total Bills
                    string queryBills = "SELECT COUNT(*) FROM MaintenanceBills";
                    SqlCommand cmdBills = new SqlCommand(queryBills, conn);
                    lblTotalBills.Text = cmdBills.ExecuteScalar().ToString();

                    // 3. Total Allotment (Unit Occupancies)
                    string queryAllotment = "SELECT COUNT(*) FROM UnitOccupancies WHERE EndDate IS NULL";
                    SqlCommand cmdAllotment = new SqlCommand(queryAllotment, conn);
                    lblTotalAllotment.Text = cmdAllotment.ExecuteScalar().ToString();

                    // 4. Total In-process Complaints (Status = 'Open' or 'In Progress')
                    string queryInProcess = "SELECT COUNT(*) FROM Complaints WHERE Status IN ('Open', 'In Progress')";
                    SqlCommand cmdInProcess = new SqlCommand(queryInProcess, conn);
                    lblInProcessComplaints.Text = cmdInProcess.ExecuteScalar().ToString();

                    // 5. Total Visitors (from GateLogs)
                    string queryVisitors = "SELECT COUNT(*) FROM GateLogs";
                    SqlCommand cmdVisitors = new SqlCommand(queryVisitors, conn);
                    lblTotalVisitors.Text = cmdVisitors.ExecuteScalar().ToString();

                    // 6. Total Unresolved Complaints (Status = 'Open')
                    string queryUnresolved = "SELECT COUNT(*) FROM Complaints WHERE Status = 'Open'";
                    SqlCommand cmdUnresolved = new SqlCommand(queryUnresolved, conn);
                    lblUnresolvedComplaints.Text = cmdUnresolved.ExecuteScalar().ToString();

                    // 7. Total Resolved Complaints (Status = 'Resolved' or 'Closed')
                    string queryResolved = "SELECT COUNT(*) FROM Complaints WHERE Status IN ('Resolved', 'Closed')";
                    SqlCommand cmdResolved = new SqlCommand(queryResolved, conn);
                    lblResolvedComplaints.Text = cmdResolved.ExecuteScalar().ToString();

                    // 8. Total Complaints
                    string queryTotal = "SELECT COUNT(*) FROM Complaints";
                    SqlCommand cmdTotal = new SqlCommand(queryTotal, conn);
                    lblTotalComplaints.Text = cmdTotal.ExecuteScalar().ToString();
                }
            }
            catch (Exception ex)
            {
                // Log error or display message
                // For production, implement proper error logging
                System.Diagnostics.Debug.WriteLine("Dashboard Error: " + ex.Message);
            }
        }
    }
}
