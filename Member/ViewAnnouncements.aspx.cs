using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SocietyManagement.Member
{
    public partial class ViewAnnouncements : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadAnnouncements();
            }
        }

        private void LoadAnnouncements()
        {
            string constr = ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = @"SELECT Title, Content, VisibleFrom, VisibleTo 
                                 FROM Announcements
                                 WHERE GETDATE() BETWEEN VisibleFrom AND VisibleTo
                                 ORDER BY VisibleFrom DESC";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        rptAnnouncements.DataSource = dt;
                        rptAnnouncements.DataBind();
                    }
                    else
                    {
                        lblNoAnnouncements.Visible = true;
                    }
                }
            }
        }
    }
}
