using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace SocietyManagement.Admin.Complaints
{
    public partial class ViewComplaints : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                if (Request.QueryString["id"] != null)
                {
                    long complaintId = Convert.ToInt64(Request.QueryString["id"]);
                    LoadComplaintDetails(complaintId);
                    LoadComments(complaintId);
                }
                else
                {
                    Response.Redirect("ComplaintDetails.aspx");
                }
            }
        }

        private void LoadComplaintDetails(long complaintId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        c.ComplaintId,
                        m.FullName AS MemberName,
                        u.UnitNo AS FlatNumber,
                        c.Title,
                        c.Description,
                        c.Status,
                        c.CreatedAt
                    FROM Complaints c
                    LEFT JOIN Users us ON c.RaisedByUserId = us.UserId
                    LEFT JOIN Members m ON us.MemberId = m.MemberId
                    LEFT JOIN Units u ON c.UnitId = u.UnitId
                    WHERE c.ComplaintId = @ComplaintId";

                SqlDataAdapter da = new SqlDataAdapter(query, con);
                da.SelectCommand.Parameters.AddWithValue("@ComplaintId", complaintId);
                DataTable dt = new DataTable();
                da.Fill(dt);

                fvComplaint.DataSource = dt;
                fvComplaint.DataBind();
            }
        }

        protected void fvComplaint_ModeChanging(object sender, FormViewModeEventArgs e)
        {
            fvComplaint.ChangeMode(e.NewMode);
            long complaintId = Convert.ToInt64(Request.QueryString["id"]);
            LoadComplaintDetails(complaintId);
        }

        protected void fvComplaint_ItemUpdating(object sender, FormViewUpdateEventArgs e)
        {
            HiddenField hdnComplaintId = (HiddenField)fvComplaint.FindControl("hdnComplaintId");
            TextBox txtTitle = (TextBox)fvComplaint.FindControl("txtTitle");
            TextBox txtDescription = (TextBox)fvComplaint.FindControl("txtDescription");
            DropDownList ddlStatus = (DropDownList)fvComplaint.FindControl("ddlStatus");

            long complaintId = Convert.ToInt64(hdnComplaintId.Value);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"UPDATE Complaints 
                                 SET Title = @Title, Description = @Description, Status = @Status
                                 WHERE ComplaintId = @ComplaintId";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Title", txtTitle.Text.Trim());
                cmd.Parameters.AddWithValue("@Description", txtDescription.Text.Trim());
                cmd.Parameters.AddWithValue("@Status", ddlStatus.SelectedValue);
                cmd.Parameters.AddWithValue("@ComplaintId", complaintId);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            fvComplaint.ChangeMode(FormViewMode.ReadOnly);
            LoadComplaintDetails(complaintId);
        }

        private void LoadComments(long complaintId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT cc.Comment, cc.CreatedAt, u.Username
                    FROM ComplaintComments cc
                    INNER JOIN Users u ON cc.UserId = u.UserId
                    WHERE cc.ComplaintId = @ComplaintId
                    ORDER BY cc.CreatedAt ASC";

                SqlDataAdapter da = new SqlDataAdapter(query, con);
                da.SelectCommand.Parameters.AddWithValue("@ComplaintId", complaintId);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptComments.DataSource = dt;
                rptComments.DataBind();

                lblNoComments.Visible = (dt.Rows.Count == 0);
            }
        }

        protected void btnAddComment_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtComment.Text))
            {
                lblMsg.Text = "Please enter a comment.";
                lblMsg.CssClass = "text-danger";
                return;
            }

            long complaintId = Convert.ToInt64(Request.QueryString["id"]);
            long userId = Convert.ToInt64(Session["UserId"]);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO ComplaintComments (ComplaintId, UserId, Comment) VALUES (@ComplaintId, @UserId, @Comment)";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ComplaintId", complaintId);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@Comment", txtComment.Text.Trim());

                con.Open();
                cmd.ExecuteNonQuery();
            }

            lblMsg.Text = "Comment added successfully!";
            lblMsg.CssClass = "text-success";
            txtComment.Text = "";

            LoadComments(complaintId);
        }
    }
}
