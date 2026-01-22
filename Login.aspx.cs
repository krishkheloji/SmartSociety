using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SocietyManagement
{
    public partial class Login : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Session.Clear(); // clear any existing session
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim(); // assuming plain text for now

            if (username == "" || password == "")
            {
                lblMessage.Text = "Please enter username and password.";
                return;
            }

            try
            {
                con.Open();
                string query = @"SELECT U.UserId, U.MemberId, U.Username, U.PasswordHash, U.IsActive, 
                                 UR.RoleId, R.Name AS RoleName, UR.SocietyId, M.FullName
                                 FROM Users U
                                 LEFT JOIN UserRoles UR ON U.UserId = UR.UserId
                                 LEFT JOIN Roles R ON UR.RoleId = R.RoleId
                                 LEFT JOIN Members M ON U.MemberId = M.MemberId
                                 WHERE U.Username = @Username AND U.IsActive = 1";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Username", username);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    // Validate password (for demo — plain text)
                    string dbPassword = dt.Rows[0]["PasswordHash"].ToString();

                    if (password == dbPassword)
                    {
                        // ✅ Create session
                        Session["UserId"] = dt.Rows[0]["UserId"].ToString();
                        Session["Username"] = dt.Rows[0]["Username"].ToString();
                        Session["FullName"] = dt.Rows[0]["FullName"].ToString();
                        Session["Role"] = dt.Rows[0]["RoleName"].ToString();
                        Session["SocietyId"] = dt.Rows[0]["SocietyId"].ToString();
                        Session["MemberId"] = dt.Rows[0]["MemberId"].ToString();


                        // ✅ Update last login
                        SqlCommand updateCmd = new SqlCommand("UPDATE Users SET LastLoginAt = GETDATE() WHERE UserId = @UserId", con);
                        updateCmd.Parameters.AddWithValue("@UserId", dt.Rows[0]["UserId"]);
                        updateCmd.ExecuteNonQuery();

                        // ✅ Redirect
                        if (Session["Role"].ToString() == "Admin")
                            Response.Redirect("~/Admin/Dashboard.aspx");
                        else
                            Response.Redirect("~/Member/ViewAnnouncements.aspx");
                    }
                    else
                    {
                        lblMessage.Text = "Invalid password!";
                    }
                }
                else
                {
                    lblMessage.Text = "User not found or inactive.";
                }
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