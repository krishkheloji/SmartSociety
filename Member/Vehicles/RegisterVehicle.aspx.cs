using System;
using System.Configuration;
using System.Data.SqlClient;

namespace SocietyManagement.Member.Vehicles
{
    public partial class RegisterVehicle : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Username"] == null || Session["Role"] == null || Session["Role"].ToString() != "Member")
            {
                Response.Redirect("~/Login.aspx");
            }
        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            if (Session["MemberId"] == null)
            {
                lblMessage.Text = "Session expired. Please log in again.";
                return;
            }

            string regNo = txtRegNo.Text.Trim().ToUpper();
            string type = ddlType.SelectedValue;
            long memberId = Convert.ToInt64(Session["MemberId"]);

            if (string.IsNullOrEmpty(regNo))
            {
                lblMessage.Text = "Please enter vehicle number.";
                return;
            }

            try
            {
                con.Open();

                // 🔹 Step 1: Get UnitId for this member (simple query)
                string getUnit = "SELECT TOP 1 UnitId FROM UnitOccupancies WHERE MemberId = @MemberId ORDER BY StartDate DESC";
                SqlCommand unitCmd = new SqlCommand(getUnit, con);
                unitCmd.Parameters.AddWithValue("@MemberId", memberId);
                object unitResult = unitCmd.ExecuteScalar();

                if (unitResult == null)
                {
                    lblMessage.Text = "No unit found for this member. Contact admin.";
                    return;
                }

                long unitId = Convert.ToInt64(unitResult);

                // 🔹 Step 2: Check if reg no already exists
                string check = "SELECT COUNT(*) FROM Vehicles WHERE RegistrationNo = @RegNo";
                SqlCommand checkCmd = new SqlCommand(check, con);
                checkCmd.Parameters.AddWithValue("@RegNo", regNo);
                int exists = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (exists > 0)
                {
                    lblMessage.Text = "This vehicle is already registered.";
                    return;
                }

                // 🔹 Step 3: Insert new vehicle
                string insert = "INSERT INTO Vehicles (MemberId, UnitId, RegistrationNo, Type) VALUES (@MemberId, @UnitId, @RegNo, @Type)";
                SqlCommand insertCmd = new SqlCommand(insert, con);
                insertCmd.Parameters.AddWithValue("@MemberId", memberId);
                insertCmd.Parameters.AddWithValue("@UnitId", unitId);
                insertCmd.Parameters.AddWithValue("@RegNo", regNo);
                insertCmd.Parameters.AddWithValue("@Type", type);
                insertCmd.ExecuteNonQuery();

                lblMessage.Text = "✅ Vehicle registered successfully!";
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error: " + ex.Message;
            }
            finally
            {
                con.Close();
            }
        }
    }
}
