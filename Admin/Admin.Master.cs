using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SocietyManagement.Admin
{
    public partial class Admin : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // ✅ Check if user session exists
            if (Session["UserId"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            // ✅ Allow only Admins
            if (Session["Role"].ToString() != "Admin")
            {
                Response.Write("<script>alert('Access denied! Admins only.'); window.location='~/Login.aspx';</script>");
                return;
            }

            // ✅ Optional: Display username
            lblAdminName.Text = "Welcome, " + Session["Username"].ToString();
        }
    }
}