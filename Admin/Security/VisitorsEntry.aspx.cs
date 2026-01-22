using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SocietyManagement.Admin.Security
{
    public partial class VisitorsEntry : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadSocieties();
            }
        }

        private void LoadSocieties()
        {
            ddlSociety.Items.Clear();
            ddlSociety.Items.Add(new ListItem("-- Select Society --", ""));

            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT SocietyId, Name FROM Societies ORDER BY Name", con);
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    ddlSociety.Items.Add(new ListItem(dr["Name"].ToString(), dr["SocietyId"].ToString()));
                }

                dr.Close();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error loading societies: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
            finally
            {
                con.Close();
            }
        }

        protected void ddlSociety_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlBuilding.Items.Clear();
            ddlBuilding.Items.Add(new ListItem("-- Select Building --", ""));
            ddlUnit.Items.Clear();
            ddlUnit.Items.Add(new ListItem("-- Select Unit --", ""));

            if (!string.IsNullOrEmpty(ddlSociety.SelectedValue))
            {
                try
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT BuildingId, Name FROM Buildings WHERE SocietyId = @SocietyId ORDER BY Name", con);
                    cmd.Parameters.AddWithValue("@SocietyId", ddlSociety.SelectedValue);
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        ddlBuilding.Items.Add(new ListItem(dr["Name"].ToString(), dr["BuildingId"].ToString()));
                    }

                    dr.Close();
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "Error loading buildings: " + ex.Message;
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
                finally
                {
                    con.Close();
                }
            }
        }

        protected void ddlBuilding_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlUnit.Items.Clear();
            ddlUnit.Items.Add(new ListItem("-- Select Unit --", ""));

            if (!string.IsNullOrEmpty(ddlBuilding.SelectedValue))
            {
                try
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT UnitId, UnitNo FROM Units WHERE BuildingId = @BuildingId ORDER BY UnitNo", con);
                    cmd.Parameters.AddWithValue("@BuildingId", ddlBuilding.SelectedValue);
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        ddlUnit.Items.Add(new ListItem(dr["UnitNo"].ToString(), dr["UnitId"].ToString()));
                    }

                    dr.Close();
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "Error loading units: " + ex.Message;
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
                finally
                {
                    con.Close();
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtVisitorName.Text))
            {
                lblMessage.Text = "Visitor name is required.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            if (string.IsNullOrEmpty(ddlSociety.SelectedValue))
            {
                lblMessage.Text = "Please select a society.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            try
            {
                con.Open();

                string query = @"INSERT INTO GateLogs 
                                (SocietyId, VisitorName, VehicleNo, Purpose, UnitId, CheckIn)
                                 VALUES (@SocietyId, @VisitorName, @VehicleNo, @Purpose, @UnitId, GETDATE())";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@SocietyId", ddlSociety.SelectedValue);
                cmd.Parameters.AddWithValue("@VisitorName", txtVisitorName.Text.Trim());
                cmd.Parameters.AddWithValue("@VehicleNo", txtVehicleNo.Text.Trim());
                cmd.Parameters.AddWithValue("@Purpose", txtPurpose.Text.Trim());

                if (string.IsNullOrEmpty(ddlUnit.SelectedValue))
                    cmd.Parameters.AddWithValue("@UnitId", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@UnitId", ddlUnit.SelectedValue);

                cmd.ExecuteNonQuery();

                lblMessage.Text = "✅ Visitor entry added successfully!";
                lblMessage.ForeColor = System.Drawing.Color.Green;

                // Clear form
                ddlSociety.SelectedIndex = 0;
                ddlBuilding.Items.Clear();
                ddlUnit.Items.Clear();
                txtVisitorName.Text = "";
                txtVehicleNo.Text = "";
                txtPurpose.Text = "";
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error saving visitor: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
            finally
            {
                con.Close();
            }
        }
    }
}
