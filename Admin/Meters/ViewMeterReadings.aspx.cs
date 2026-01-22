using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SocietyManagement.Admin.Meters
{
    public partial class ViewMeterReadings : System.Web.UI.Page
    {
        string cs = ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadMeterReadings();
            }
        }

        // 🔹 Load all meter readings with unit details
        private void LoadMeterReadings()
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        MR.ReadingId,
                        U.UnitNo,
                        MR.ReadingDate,
                        MR.RatePerUnit,
                        MR.ReadingValue
                    FROM MeterReadings MR
                    INNER JOIN Units U ON MR.UnitId = U.UnitId
                    ORDER BY MR.ReadingDate DESC", con);

                con.Open();
                gvMeterReadings.DataSource = cmd.ExecuteReader();
                gvMeterReadings.DataBind();
            }
        }

        // 🔹 Handle Delete button click
        protected void gvMeterReadings_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DeleteReading")
            {
                int readingId = Convert.ToInt32(e.CommandArgument);
                DeleteMeterReading(readingId);
            }
        }

        // 🔹 Delete meter reading
        private void DeleteMeterReading(int readingId)
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("DELETE FROM MeterReadings WHERE ReadingId = @Id", con);
                cmd.Parameters.AddWithValue("@Id", readingId);
                con.Open();

                int rows = cmd.ExecuteNonQuery();

                if (rows > 0)
                {
                    lblMessage.CssClass = "text-success";
                    lblMessage.Text = "✅ Meter reading deleted successfully.";
                    LoadMeterReadings();
                }
                else
                {
                    lblMessage.CssClass = "text-danger";
                    lblMessage.Text = "❌ Failed to delete meter reading.";
                }
            }
        }
    }
}