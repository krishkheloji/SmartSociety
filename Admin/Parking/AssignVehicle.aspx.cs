using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace SocietyManagement.Admin.Parking
{
    public partial class AssignVehicle : System.Web.UI.Page
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString);

        protected void Page_Load(object sender, EventArgs e)
        {
            CheckAdminAuth();
            if (!IsPostBack)
            {
                LoadSlots();
                LoadVehicles();
                LoadAssignments();
            }
        }
        private void CheckAdminAuth()
        {
            if (Session["Username"] == null || Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                Response.Redirect("~/Login.aspx");
            }
        }

        private void LoadSlots()
        {
            SqlDataAdapter da = new SqlDataAdapter("SELECT SlotId, Identifier FROM ParkingSlots", conn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            ddlSlots.DataSource = dt;
            ddlSlots.DataTextField = "Identifier";
            ddlSlots.DataValueField = "SlotId";
            ddlSlots.DataBind();
        }

        private void LoadVehicles()
        {
            SqlDataAdapter da = new SqlDataAdapter(@"
                SELECT v.VehicleId, 
                       v.RegistrationNo + ' (' + m.FullName + ')' AS VehicleDisplay
                FROM Vehicles v
                INNER JOIN Members m ON v.MemberId = m.MemberId", conn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            ddlVehicles.DataSource = dt;
            ddlVehicles.DataTextField = "VehicleDisplay";
            ddlVehicles.DataValueField = "VehicleId";
            ddlVehicles.DataBind();
        }

        private void LoadAssignments()
        {
            string query = @"
                SELECT 
                    pa.AssignmentId,
                    ps.Identifier AS SlotIdentifier,
                    v.RegistrationNo,
                    m.FullName AS MemberName,
                    pa.StartDate,
                    pa.EndDate
                FROM ParkingAssignments pa
                INNER JOIN Vehicles v ON pa.VehicleId = v.VehicleId
                INNER JOIN Members m ON v.MemberId = m.MemberId
                INNER JOIN ParkingSlots ps ON pa.SlotId = ps.SlotId
                ORDER BY pa.AssignmentId DESC";

            SqlDataAdapter da = new SqlDataAdapter(query, conn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            gvAssignments.DataSource = dt;
            gvAssignments.DataBind();
        }

        protected void btnAssign_Click(object sender, EventArgs e)
        {
            // Step 1: Basic validation
            if (string.IsNullOrEmpty(txtStartDate.Text))
            {
                lblMessage.CssClass = "d-block mt-3 fw-bold text-danger";
                lblMessage.Text = "Please select a start date.";
                return;
            }

            string startDate = txtStartDate.Text;
            string endDate = string.IsNullOrEmpty(txtEndDate.Text) ? null : txtEndDate.Text;

            conn.Open();

            // Step 2: Find which member owns the selected vehicle
            SqlCommand getMemberCmd = new SqlCommand(
                "SELECT MemberId FROM Vehicles WHERE VehicleId = @VehicleId", conn);
            getMemberCmd.Parameters.AddWithValue("@VehicleId", ddlVehicles.SelectedValue);
            long memberId = Convert.ToInt64(getMemberCmd.ExecuteScalar());

            // Step 2: Check if the selected slot is already assigned (still active)
            string slotCheckQuery = @"
    SELECT COUNT(*) 
    FROM ParkingAssignments
    WHERE SlotId = @SlotId
      AND (EndDate IS NULL OR EndDate >= CAST(GETDATE() AS DATE))";

            SqlCommand checkSlotCmd = new SqlCommand(slotCheckQuery, conn);
            checkSlotCmd.Parameters.AddWithValue("@SlotId", ddlSlots.SelectedValue);

            int activeSlotCount = Convert.ToInt32(checkSlotCmd.ExecuteScalar());

            if (activeSlotCount > 0)
            {
                conn.Close();
                lblMessage.CssClass = "d-block mt-3 fw-bold text-danger";
                lblMessage.Text = "❌ This parking slot is already assigned.";
                return;
            }

            // Step 4: Check if this vehicle is already assigned (active assignment)
            SqlCommand checkVehicle = new SqlCommand(@"
        SELECT COUNT(*) 
        FROM ParkingAssignments
        WHERE VehicleId = @VehicleId
          AND (EndDate IS NULL OR EndDate >= CAST(GETDATE() AS DATE))", conn);
            checkVehicle.Parameters.AddWithValue("@VehicleId", ddlVehicles.SelectedValue);

            int vehicleConflict = Convert.ToInt32(checkVehicle.ExecuteScalar());

            if (vehicleConflict > 0)
            {
                conn.Close();
                lblMessage.CssClass = "d-block mt-3 fw-bold text-danger";
                lblMessage.Text = "❌ This vehicle is already assigned to another slot.";
                return;
            }

            // Step 5: Assign the vehicle to the slot
            SqlCommand insertCmd = new SqlCommand(@"
        INSERT INTO ParkingAssignments (SlotId, VehicleId, StartDate, EndDate)
        VALUES (@SlotId, @VehicleId, @StartDate, @EndDate)", conn);

            insertCmd.Parameters.AddWithValue("@SlotId", ddlSlots.SelectedValue);
            insertCmd.Parameters.AddWithValue("@VehicleId", ddlVehicles.SelectedValue);
            insertCmd.Parameters.AddWithValue("@StartDate", startDate);
            insertCmd.Parameters.AddWithValue("@EndDate", (object)endDate ?? DBNull.Value);

            insertCmd.ExecuteNonQuery();
            conn.Close();

            // Step 6: Reset and show success
            txtStartDate.Text = "";
            txtEndDate.Text = "";
            lblMessage.CssClass = "d-block mt-3 fw-bold text-success";
            lblMessage.Text = "✅ Vehicle assigned successfully!";

            LoadAssignments();
        }



        // 🗑️ NEW: Delete assignment
        protected void gvAssignments_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int assignmentId = Convert.ToInt32(gvAssignments.DataKeys[e.RowIndex].Value);

            SqlCommand cmd = new SqlCommand("DELETE FROM ParkingAssignments WHERE AssignmentId=@AssignmentId", conn);
            cmd.Parameters.AddWithValue("@AssignmentId", assignmentId);

            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

            LoadAssignments();
        }

    }
}