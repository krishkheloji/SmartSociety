using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace SocietyManagement.Admin.Amenities
{
    public partial class ManageAmenities : System.Web.UI.Page
    {
        private string ConnString => ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Username"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            // Only admins can access this page
            if (!Session["Role"].ToString().Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/AccessDenied.aspx");
                return;
            }

            if (!IsPostBack)
                BindAmenities();
        }

        private long GetCurrentSocietyId()
        {
            // Derive the SocietyId from logged-in user's record
            using (var con = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(@"
                SELECT m.SocietyId 
                FROM Users u
                INNER JOIN Members m ON u.MemberId = m.MemberId
                WHERE u.Username = @Username", con))
            {
                cmd.Parameters.AddWithValue("@Username", Session["Username"].ToString());
                con.Open();
                object result = cmd.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                    throw new Exception("Unable to find society for current user.");

                return Convert.ToInt64(result);
            }
        }

        private void BindAmenities()
        {
            try
            {
                long societyId = GetCurrentSocietyId();
                using (var con = new SqlConnection(ConnString))
                using (var cmd = new SqlCommand(@"
                    SELECT AmenityId, Name, BookingRequired 
                    FROM Amenities 
                    WHERE SocietyId = @SocietyId 
                    ORDER BY Name", con))
                {
                    cmd.Parameters.AddWithValue("@SocietyId", societyId);
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        gvAmenities.DataSource = dt;
                        gvAmenities.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠ Error loading amenities: " + ex.Message;
            }
        }

        protected void btnAdd_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "";
            string name = txtName.Text.Trim();
            bool bookingRequired = chkBookingRequired.Checked;

            if (string.IsNullOrEmpty(name))
            {
                lblMessage.Text = "Please enter an amenity name.";
                return;
            }

            try
            {
                long societyId = GetCurrentSocietyId();

                if (!string.IsNullOrEmpty(hfEditAmenityId.Value))
                {
                    // update existing record
                    long amenityId = Convert.ToInt64(hfEditAmenityId.Value);
                    using (var con = new SqlConnection(ConnString))
                    using (var cmd = new SqlCommand(@"
                UPDATE Amenities
                SET Name = @Name, BookingRequired = @BookingRequired
                WHERE AmenityId = @AmenityId", con))
                    {
                        cmd.Parameters.AddWithValue("@Name", name);
                        cmd.Parameters.AddWithValue("@BookingRequired", bookingRequired);
                        cmd.Parameters.AddWithValue("@AmenityId", amenityId);

                        con.Open();
                        cmd.ExecuteNonQuery();
                    }

                    lblMessage.Text = "✅ Amenity updated successfully.";
                }
                else
                {
                    // insert new record
                    using (var con = new SqlConnection(ConnString))
                    using (var cmd = new SqlCommand(@"
                INSERT INTO Amenities (SocietyId, Name, BookingRequired)
                VALUES (@SocietyId, @Name, @BookingRequired)", con))
                    {
                        cmd.Parameters.AddWithValue("@SocietyId", societyId);
                        cmd.Parameters.AddWithValue("@Name", name);
                        cmd.Parameters.AddWithValue("@BookingRequired", bookingRequired);

                        con.Open();
                        cmd.ExecuteNonQuery();
                    }

                    lblMessage.Text = "✅ Amenity added successfully.";
                }

                txtName.Text = "";
                chkBookingRequired.Checked = false;
                hfEditAmenityId.Value = "";
                btnCancelEdit.Visible = false;
                btnAdd.Text = "Add Amenity";

                BindAmenities();
            }
            catch (SqlException sqx) when (sqx.Number == 2627)
            {
                lblMessage.Text = "⚠ Amenity with this name already exists.";
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠ Error saving amenity: " + ex.Message;
            }
        }


        protected void gvAmenities_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvAmenities.PageIndex = e.NewPageIndex;
            BindAmenities();
        }

        protected void gvAmenities_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvAmenities.EditIndex = e.NewEditIndex;
            BindAmenities();
        }

        protected void gvAmenities_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvAmenities.EditIndex = -1;
            BindAmenities();
        }

        protected void gvAmenities_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            lblMessage.Text = "";
            int rowIndex = e.RowIndex;
            long amenityId = Convert.ToInt64(gvAmenities.DataKeys[rowIndex].Value);

            var row = gvAmenities.Rows[rowIndex];
            var txtEditName = (System.Web.UI.WebControls.TextBox)row.FindControl("txtEditName");
            var chkEditBooking = (System.Web.UI.WebControls.CheckBox)row.FindControl("chkEditBooking");

            string newName = txtEditName.Text.Trim();
            bool bookingReq = chkEditBooking.Checked;

            if (string.IsNullOrEmpty(newName))
            {
                lblMessage.Text = "Amenity name cannot be empty.";
                return;
            }

            try
            {
                using (var con = new SqlConnection(ConnString))
                using (var cmd = new SqlCommand(@"
                    UPDATE Amenities
                    SET Name = @Name, BookingRequired = @BookingRequired
                    WHERE AmenityId = @AmenityId", con))
                {
                    cmd.Parameters.AddWithValue("@Name", newName);
                    cmd.Parameters.AddWithValue("@BookingRequired", bookingReq);
                    cmd.Parameters.AddWithValue("@AmenityId", amenityId);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                gvAmenities.EditIndex = -1;
                BindAmenities();
                lblMessage.Text = "✅ Amenity updated successfully.";
            }
            catch (SqlException sqx) when (sqx.Number == 2627)
            {
                lblMessage.Text = "⚠ Another amenity with this name exists.";
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠ Error updating amenity: " + ex.Message;
            }
        }

        protected void gvAmenities_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            lblMessage.Text = "";
            long amenityId = Convert.ToInt64(gvAmenities.DataKeys[e.RowIndex].Value);

            try
            {
                using (var con = new SqlConnection(ConnString))
                using (var cmd = new SqlCommand("DELETE FROM Amenities WHERE AmenityId = @AmenityId", con))
                {
                    cmd.Parameters.AddWithValue("@AmenityId", amenityId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                BindAmenities();
                lblMessage.Text = "✅ Amenity deleted.";
            }
            catch (SqlException)
            {
                lblMessage.Text = "⚠ Cannot delete this amenity — it may have active bookings.";
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠ Error deleting amenity: " + ex.Message;
            }
        }
        protected void btnCancelEdit_Click(object sender, EventArgs e) 
        { 
            txtName.Text = ""; 
            chkBookingRequired.Checked = false; 
            hfEditAmenityId.Value = ""; 
            btnCancelEdit.Visible = false; 
        }

        protected void gvAmenities_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            lblMessage.Text = "";

            if (e.CommandName == "EditRow")
            {
                long amenityId = Convert.ToInt64(e.CommandArgument);
                gvAmenities.EditIndex = -1; // reset edit index just in case
                EnterEditMode(amenityId);
            }
            else if (e.CommandName == "DeleteRow")
            {
                long amenityId = Convert.ToInt64(e.CommandArgument);
                DeleteAmenity(amenityId);
            }
        }

        private void EnterEditMode(long amenityId)
        {
            try
            {
                using (var con = new SqlConnection(ConnString))
                using (var cmd = new SqlCommand(@"
            SELECT Name, BookingRequired
            FROM Amenities
            WHERE AmenityId = @AmenityId", con))
                {
                    cmd.Parameters.AddWithValue("@AmenityId", amenityId);
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            hfEditAmenityId.Value = amenityId.ToString();
                            txtName.Text = reader["Name"].ToString();
                            chkBookingRequired.Checked = Convert.ToBoolean(reader["BookingRequired"]);
                            btnAdd.Text = "Update Amenity";
                            btnCancelEdit.Visible = true;
                        }
                        else
                        {
                            lblMessage.Text = "⚠ Amenity not found.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠ Error entering edit mode: " + ex.Message;
            }
        }

        private void DeleteAmenity(long amenityId)
        {
            try
            {
                using (var con = new SqlConnection(ConnString))
                using (var cmd = new SqlCommand("DELETE FROM Amenities WHERE AmenityId = @AmenityId", con))
                {
                    cmd.Parameters.AddWithValue("@AmenityId", amenityId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                BindAmenities();
                lblMessage.Text = "✅ Amenity deleted successfully.";
            }
            catch (SqlException)
            {
                lblMessage.Text = "⚠ Cannot delete this amenity. It might have active bookings.";
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠ Error deleting amenity: " + ex.Message;
            }
        }




    }
}
