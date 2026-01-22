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
    public partial class AddMeterReading : System.Web.UI.Page
    {
        string cs = ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadUnits();
            }
        }

        private void LoadUnits()
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("SELECT UnitId, UnitNo FROM Units ORDER BY UnitNo", con);
                con.Open();
                ddlUnit.DataSource = cmd.ExecuteReader();
                ddlUnit.DataTextField = "UnitNo";
                ddlUnit.DataValueField = "UnitId";
                ddlUnit.DataBind();
            }
            ddlUnit.Items.Insert(0, new ListItem("-- Select Unit --", "0"));
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (ddlUnit.SelectedIndex == 0)
            {
                lblMessage.CssClass = "text-danger";
                lblMessage.Text = "⚠️ Please select a unit.";
                return;
            }

            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO MeterReadings (UnitId, ReadingDate, RatePerUnit, ReadingValue)
                    VALUES (@UnitId, @ReadingDate, @RatePerUnit, @ReadingValue)", con);

                cmd.Parameters.AddWithValue("@UnitId", ddlUnit.SelectedValue);
                cmd.Parameters.AddWithValue("@ReadingDate", txtReadingDate.Text);
                cmd.Parameters.AddWithValue("@RatePerUnit", txtRate.Text);
                cmd.Parameters.AddWithValue("@ReadingValue", txtValue.Text);

                con.Open();
                int rows = cmd.ExecuteNonQuery();

                if (rows > 0)
                {
                    lblMessage.CssClass = "text-success";
                    lblMessage.Text = "✅ Meter reading added successfully!";
                    ClearForm();
                }
                else
                {
                    lblMessage.CssClass = "text-danger";
                    lblMessage.Text = "❌ Failed to add reading.";
                }
            }
        }

        private void ClearForm()
        {
            ddlUnit.SelectedIndex = 0;
            txtReadingDate.Text = "";
            txtRate.Text = "";
            txtValue.Text = "";
        }
    }

}