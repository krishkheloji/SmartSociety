using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace SocietyManagement.Admin.Member
{
    public partial class ManageMember : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindMembers();
            }
        }

        private void BindMembers()
        {
            SqlDataAdapter da = new SqlDataAdapter("SELECT MemberId, FullName, Email, Phone, Status, CreatedAt FROM Members ORDER BY MemberId DESC", con);
            DataTable dt = new DataTable();
            da.Fill(dt);
            gvMembers.DataSource = dt;
            gvMembers.DataBind();
        }

        protected void gvMembers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DeleteRow")
            {
                long memberId = Convert.ToInt64(e.CommandArgument);
                con.Open();

                // 1️⃣ Delete UserRoles linked to that member's Users
                SqlCommand cmdRoles = new SqlCommand(@"
            DELETE UR 
            FROM UserRoles UR
            INNER JOIN Users U ON UR.UserId = U.UserId
            WHERE U.MemberId = @MemberId", con);
                cmdRoles.Parameters.AddWithValue("@MemberId", memberId);
                cmdRoles.ExecuteNonQuery();

                // 2️⃣ Delete Users linked to that member
                SqlCommand cmdUsers = new SqlCommand("DELETE FROM Users WHERE MemberId=@MemberId", con);
                cmdUsers.Parameters.AddWithValue("@MemberId", memberId);
                cmdUsers.ExecuteNonQuery();

                // 3️⃣ Delete from UnitOccupancies
                SqlCommand cmdOcc = new SqlCommand("DELETE FROM UnitOccupancies WHERE MemberId=@MemberId", con);
                cmdOcc.Parameters.AddWithValue("@MemberId", memberId);
                cmdOcc.ExecuteNonQuery();

                // 4️⃣ Finally delete Member
                SqlCommand cmdMembers = new SqlCommand("DELETE FROM Members WHERE MemberId=@MemberId", con);
                cmdMembers.Parameters.AddWithValue("@MemberId", memberId);
                cmdMembers.ExecuteNonQuery();

                con.Close();

                lblMsg.Text = "❌ Member deleted successfully.";
                lblMsg.CssClass = "text-danger";
                BindMembers();
            }
            else if (e.CommandName == "EditRow")
            {
                string memberId = e.CommandArgument.ToString();
                Response.Redirect("~/Admin/Member/AddMember.aspx?MemberId=" + memberId);
            }
        }

    }
}
