using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SocietyManagement.Member.Visitors
{
    public partial class GateLogs : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {

            if (Session["SocietyId"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                //bool canManageLogs = IsUserAllowedToManageLogs();

                //pnlAddLog.Visible = canManageLogs;
                //lblAddNotAllowed.Visible = !canManageLogs;

                LoadUnits();
                LoadGateLogs();
            }
        }


        //private bool IsUserAllowedToManageLogs()
        //{
        //    string role = Session["Role"]?.ToString() ?? "";
        //    return role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
        //        || role.Equals("Security", StringComparison.OrdinalIgnoreCase);
        //}


        private void LoadUnits()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT UnitId, UnitNo FROM Units WHERE BuildingId IN (SELECT BuildingId FROM Buildings WHERE SocietyId = @SocietyId)",
                    con))
                {
                    cmd.Parameters.AddWithValue("@SocietyId", Session["SocietyId"]);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    ddlUnits.DataSource = dt;
                    ddlUnits.DataTextField = "UnitNo";
                    ddlUnits.DataValueField = "UnitId";
                    ddlUnits.DataBind();

                    ddlUnits.Items.Insert(0, new System.Web.UI.WebControls.ListItem("-- Select Unit --", ""));
                }
            }
            catch (Exception ex)
            {
                lblMessage.CssClass = "text-danger fw-semibold";
                lblMessage.Text = "Error loading units: " + ex.Message;
            }
        }


        private void LoadGateLogs()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(@"
                   SELECT GL.GateLogId, GL.VisitorName, GL.VehicleNo, GL.Purpose,
                   U.UnitNo AS UnitNo, GL.CheckIn, GL.CheckOut, GL.ApprovalStatus
                FROM GateLogs GL
                INNER JOIN Units U ON GL.UnitId = U.UnitId
                INNER JOIN UnitOccupancies UO ON UO.UnitId = U.UnitId
                INNER JOIN Members M ON M.MemberId = UO.MemberId
                INNER JOIN Users USR ON USR.MemberId = M.MemberId
                WHERE USR.UserId = @UserId
                ORDER BY GL.GateLogId DESC", con))
                {
                    cmd.Parameters.AddWithValue("@UserId", Session["UserId"]);


                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvGateLogs.DataSource = dt;
                    gvGateLogs.DataBind();
                }
            }
            catch (Exception ex)
            {
                lblMessage.CssClass = "text-danger fw-semibold";
                lblMessage.Text = "Error loading gate logs: " + ex.Message;
            }
        }


        protected void btnAddLog_Click(object sender, EventArgs e)
        {
            //if (!IsUserAllowedToManageLogs())
            //{
            //    lblMessage.CssClass = "text-danger fw-semibold";
            //    lblMessage.Text = "You are not authorized to add gate logs.";
            //    return;
            //}

            if (string.IsNullOrWhiteSpace(txtVisitorName.Text))
            {
                lblMessage.CssClass = "text-danger fw-semibold";
                lblMessage.Text = "Enter visitor name.";
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO GateLogs (SocietyId, VisitorName, VehicleNo, Purpose, UnitId, CheckIn)
                    VALUES (@SocietyId, @VisitorName, @VehicleNo, @Purpose, @UnitId, GETDATE())", con))
                {
                    cmd.Parameters.AddWithValue("@SocietyId", Session["SocietyId"]);
                    cmd.Parameters.AddWithValue("@VisitorName", txtVisitorName.Text.Trim());
                    cmd.Parameters.AddWithValue("@VehicleNo", txtVehicleNo.Text.Trim());
                    cmd.Parameters.AddWithValue("@Purpose", txtPurpose.Text.Trim());

                    if (string.IsNullOrEmpty(ddlUnits.SelectedValue))
                        cmd.Parameters.AddWithValue("@UnitId", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@UnitId", Convert.ToInt64(ddlUnits.SelectedValue));

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                lblMessage.CssClass = "text-success fw-semibold";
                lblMessage.Text = "Visitor entry added successfully.";

                txtVisitorName.Text = "";
                txtVehicleNo.Text = "";
                txtPurpose.Text = "";
                ddlUnits.SelectedIndex = 0;

                LoadGateLogs();
            }
            catch (Exception ex)
            {
                lblMessage.CssClass = "text-danger fw-semibold";
                lblMessage.Text = "Error adding entry: " + ex.Message;
            }
        }


        protected void gvGateLogs_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Checkout")
            {
                //if (!IsUserAllowedToManageLogs())
                //{
                //    lblMessage.CssClass = "text-danger fw-semibold";
                //    lblMessage.Text = "You are not authorized to perform checkout.";
                //    return;
                //}

                long gateLogId = Convert.ToInt64(e.CommandArgument);

                try
                {
                    using (SqlConnection con = new SqlConnection(connectionString))
                    using (SqlCommand cmd = new SqlCommand(
                        "UPDATE GateLogs SET CheckOut = GETDATE() WHERE GateLogId = @GateLogId", con))
                    {
                        cmd.Parameters.AddWithValue("@GateLogId", gateLogId);
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }

                    lblMessage.CssClass = "text-success fw-semibold";
                    lblMessage.Text = "Visitor checked out successfully.";

                    LoadGateLogs();
                }
                catch (Exception ex)
                {
                    lblMessage.CssClass = "text-danger fw-semibold";
                    lblMessage.Text = "Error during checkout: " + ex.Message;
                }
            }
            if (e.CommandName == "Approve" || e.CommandName == "Reject")
            {
                long gateLogId = Convert.ToInt64(e.CommandArgument);
                string status = e.CommandName == "Approve" ? "Approved" : "Rejected";
                long memberId = Convert.ToInt64(Session["MemberId"]);

                try
                {
                    using (SqlConnection con = new SqlConnection(connectionString))
                    using (SqlCommand cmd = new SqlCommand(@"
            UPDATE GateLogs 
            SET ApprovalStatus = @Status
            WHERE GateLogId = @GateLogId", con))
                    {
                        cmd.Parameters.AddWithValue("@Status", status);
                        cmd.Parameters.AddWithValue("@MemberId", memberId);
                        cmd.Parameters.AddWithValue("@GateLogId", gateLogId);

                        con.Open();
                        cmd.ExecuteNonQuery();
                    }

                    lblMessage.CssClass = "text-success fw-semibold";
                    lblMessage.Text = $"Visitor {status.ToLower()} successfully.";

                    LoadGateLogs();
                }
                catch (Exception ex)
                {
                    lblMessage.CssClass = "text-danger fw-semibold";
                    lblMessage.Text = "Error updating visitor status: " + ex.Message;
                }
            }

        }
    }
}