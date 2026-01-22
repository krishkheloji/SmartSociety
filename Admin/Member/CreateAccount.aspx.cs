using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Web.UI;

namespace SocietyManagement.Admin.Member
{
    public partial class CreateAccount : System.Web.UI.Page
    {
        private string connString = ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadMembers();
                LoadRoles();
            }
        }

        private void LoadMembers()
        {
            using (SqlConnection con = new SqlConnection(connString))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("SELECT MemberId, Email FROM Members WHERE Status='Active' ORDER BY Email", con);
                    con.Open();
                    ddlMember.DataSource = cmd.ExecuteReader();
                    ddlMember.DataTextField = "Email";
                    ddlMember.DataValueField = "MemberId";
                    ddlMember.DataBind();
                    ddlMember.Items.Insert(0, new System.Web.UI.WebControls.ListItem("--Select Member Email Id--", "0"));
                }
                catch (Exception ex)
                {
                    lblMsg.Text = "❌ Error loading members: " + ex.Message;
                    lblMsg.CssClass = "text-danger";
                }
            }
        }

        private void LoadRoles()
        {
            using (SqlConnection con = new SqlConnection(connString))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("SELECT RoleId, Name FROM Roles ORDER BY Name", con);
                    con.Open();
                    ddlRole.DataSource = cmd.ExecuteReader();
                    ddlRole.DataTextField = "Name";
                    ddlRole.DataValueField = "RoleId";
                    ddlRole.DataBind();
                    ddlRole.Items.Insert(0, new System.Web.UI.WebControls.ListItem("--Select Role--", "0"));
                }
                catch (Exception ex)
                {
                    lblMsg.Text = "❌ Error loading roles: " + ex.Message;
                    lblMsg.CssClass = "text-danger";
                }
            }
        }

        protected void btnCreateUser_Click(object sender, EventArgs e)
        {
            // Clear previous message
            lblMsg.Text = "";

            // Validation
            if (ddlMember.SelectedIndex == 0 || ddlRole.SelectedIndex == 0)
            {
                lblMsg.Text = "⚠ Please select both Member Email and Role.";
                lblMsg.CssClass = "text-danger";
                return;
            }

            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                lblMsg.Text = "⚠ Username and Password are required.";
                lblMsg.CssClass = "text-danger";
                return;
            }

            if (username.Length < 3)
            {
                lblMsg.Text = "⚠ Username must be at least 3 characters long.";
                lblMsg.CssClass = "text-danger";
                return;
            }

            if (password.Length < 6)
            {
                lblMsg.Text = "⚠ Password must be at least 6 characters long.";
                lblMsg.CssClass = "text-danger";
                return;
            }

            long memberId = Convert.ToInt64(ddlMember.SelectedValue);
            int roleId = Convert.ToInt32(ddlRole.SelectedValue);
            bool isActive = ddlStatus.SelectedValue == "1";

            using (SqlConnection con = new SqlConnection(connString))
            {
                con.Open();
                SqlTransaction transaction = con.BeginTransaction();

                try
                {
                    // Check if username already exists
                    if (UsernameExists(username, con, transaction))
                    {
                        lblMsg.Text = "⚠ Username already exists. Please choose a different username.";
                        lblMsg.CssClass = "text-danger";
                        transaction.Rollback();
                        return;
                    }

                    // Check if user account already exists for this member
                    if (UserAccountExistsForMember(memberId, con, transaction))
                    {
                        lblMsg.Text = "⚠ User account already exists for this member.";
                        lblMsg.CssClass = "text-danger";
                        transaction.Rollback();
                        return;
                    }

                    // Get Society ID
                    long societyId = GetSocietyIdByMember(memberId, con, transaction);
                    if (societyId == 0)
                    {
                        lblMsg.Text = "❌ Could not find society for selected member.";
                        lblMsg.CssClass = "text-danger";
                        transaction.Rollback();
                        return;
                    }

                    // Step 1: Insert into Users table
                    SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO Users (MemberId, Username, PasswordHash, IsActive)
                        OUTPUT INSERTED.UserId
                        VALUES (@MemberId, @Username, @PasswordHash, @IsActive)", con, transaction);

                    cmd.Parameters.AddWithValue("@MemberId", memberId);
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@PasswordHash", password); // Plain password
                    cmd.Parameters.AddWithValue("@IsActive", isActive);

                    long userId = Convert.ToInt64(cmd.ExecuteScalar());

                    // Step 2: Insert into UserRoles
                    SqlCommand cmdRole = new SqlCommand(@"
                        INSERT INTO UserRoles (UserId, RoleId, SocietyId)
                        VALUES (@UserId, @RoleId, @SocietyId)", con, transaction);

                    cmdRole.Parameters.AddWithValue("@UserId", userId);
                    cmdRole.Parameters.AddWithValue("@RoleId", roleId);
                    cmdRole.Parameters.AddWithValue("@SocietyId", societyId);
                    cmdRole.ExecuteNonQuery();

                    // Commit transaction
                    transaction.Commit();

                    // Step 3: Get member email and send account details
                    string memberEmail = GetMemberEmailById(memberId);

                    if (string.IsNullOrEmpty(memberEmail))
                    {
                        lblMsg.Text = "✅ User created successfully, but email address not found.";
                        lblMsg.CssClass = "text-warning";
                        ClearForm();
                        return;
                    }

                    // Send email (use plain password for email notification)
                    bool emailSent = SendAccountEmail(memberEmail, username, password);

                    if (emailSent)
                    {
                        lblMsg.Text = "✅ User account created and credentials sent to " + memberEmail;
                        lblMsg.CssClass = "text-success";
                    }
                    else
                    {
                        lblMsg.Text = "✅ User created successfully, but email notification failed.";
                        lblMsg.CssClass = "text-warning";
                    }

                    ClearForm();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    lblMsg.Text = "❌ Error: " + ex.Message;
                    lblMsg.CssClass = "text-danger";
                }
            }
        }

        private bool UsernameExists(string username, SqlConnection con, SqlTransaction transaction)
        {
            SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Username = @Username", con, transaction);
            cmd.Parameters.AddWithValue("@Username", username);
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }

        private bool UserAccountExistsForMember(long memberId, SqlConnection con, SqlTransaction transaction)
        {
            SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE MemberId = @MemberId", con, transaction);
            cmd.Parameters.AddWithValue("@MemberId", memberId);
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }

        private long GetSocietyIdByMember(long memberId, SqlConnection con, SqlTransaction transaction)
        {
            SqlCommand cmd = new SqlCommand("SELECT SocietyId FROM Members WHERE MemberId = @MemberId", con, transaction);
            cmd.Parameters.AddWithValue("@MemberId", memberId);

            object result = cmd.ExecuteScalar();

            if (result != null && result != DBNull.Value)
                return Convert.ToInt64(result);

            return 0;
        }

        private string GetMemberEmailById(long memberId)
        {
            using (SqlConnection con = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand("SELECT Email FROM Members WHERE MemberId = @MemberId", con);
                cmd.Parameters.AddWithValue("@MemberId", memberId);

                con.Open();
                object result = cmd.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                    return result.ToString();

                return string.Empty;
            }
        }



        private bool SendAccountEmail(string toEmail, string username, string password)
        {
            try
            {
                // Get email settings from web.config
                string fromEmail = ConfigurationManager.AppSettings["EmailFrom"] ?? "parkarpeter157@gmail.com";
                string emailPassword = ConfigurationManager.AppSettings["EmailPassword"] ?? "wpqw rtvm xsbf imkv";
                string smtpHost = ConfigurationManager.AppSettings["SmtpHost"] ?? "smtp.gmail.com";
                int smtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["SmtpPort"] ?? "587");

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(fromEmail, "Society Management System");
                mail.To.Add(toEmail);
                mail.Subject = "Your Society Account Details";
                mail.IsBodyHtml = true;
                mail.Body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2 style='color: #2c3e50;'>Welcome to Society Management System</h2>
                        <p>Dear Member,</p>
                        <p>Your Society Management account has been created successfully.</p>
                        <div style='background-color: #f8f9fa; padding: 15px; border-left: 4px solid #007bff; margin: 20px 0;'>
                            <h3 style='margin-top: 0;'>Login Credentials:</h3>
                            <p><strong>Username:</strong> {username}</p>
                            <p><strong>Password:</strong> {password}</p>
                        </div>
                        <p style='color: #dc3545;'><strong>Important:</strong> Please change your password after first login for security reasons.</p>
                        <p>You can access the system at: <a href='#'>Society Management Portal</a></p>
                        <br>
                        <p>Best Regards,<br>Society Management Team</p>
                    </body>
                    </html>";

                SmtpClient smtp = new SmtpClient(smtpHost, smtpPort);
                smtp.Credentials = new NetworkCredential(fromEmail, emailPassword);
                smtp.EnableSsl = true;
                smtp.Send(mail);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Email Error: " + ex.Message);
                return false;
            }
        }

        private void ClearForm()
        {
            ddlMember.SelectedIndex = 0;
            txtUsername.Text = "";
            txtPassword.Text = "";
            ddlRole.SelectedIndex = 0;
            ddlStatus.SelectedIndex = 0;
        }
    }
}