using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SocietyManagement.Admin.Member
{
    public partial class AddMember : Page
    {
        private readonly string _connectionString = System.Configuration.ConfigurationManager
            .ConnectionStrings["dbconn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadSocieties();

                if (Request.QueryString["MemberId"] != null)
                {
                    long memberId = Convert.ToInt64(Request.QueryString["MemberId"]);
                    LoadMemberData(memberId);
                    btnSave.Text = "Update Member";
                }
            }
        }

        #region Dropdown Loaders

        private void LoadSocieties()
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT SocietyId, Name FROM Societies", con))
            {
                con.Open();
                ddlSociety.DataSource = cmd.ExecuteReader();
                ddlSociety.DataTextField = "Name";
                ddlSociety.DataValueField = "SocietyId";
                ddlSociety.DataBind();
            }

            ddlSociety.Items.Insert(0, new ListItem("-- Select Society --", "0"));
            ddlBuilding.Items.Insert(0, new ListItem("-- Select Building --", "0"));
            ddlUnit.Items.Insert(0, new ListItem("-- Select Unit --", "0"));
        }

        private void LoadBuildings(long societyId)
        {
            ddlBuilding.Items.Clear();

            using (SqlConnection con = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT BuildingId, Name FROM Buildings WHERE SocietyId=@SocietyId", con))
            {
                cmd.Parameters.AddWithValue("@SocietyId", societyId);
                con.Open();
                ddlBuilding.DataSource = cmd.ExecuteReader();
                ddlBuilding.DataTextField = "Name";
                ddlBuilding.DataValueField = "BuildingId";
                ddlBuilding.DataBind();
            }

            ddlBuilding.Items.Insert(0, new ListItem("-- Select Building --", "0"));
        }

        private void LoadUnits(long buildingId)
        {
            ddlUnit.Items.Clear();

            string query = @"
        SELECT u.UnitId, u.UnitNo
        FROM Units u
        WHERE u.BuildingId = @BuildingId
          AND u.UnitId NOT IN (
              SELECT UnitId 
              FROM UnitOccupancies 
              WHERE EndDate IS NULL
          )";

            using (SqlConnection con = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@BuildingId", buildingId);
                con.Open();
                ddlUnit.DataSource = cmd.ExecuteReader();
                ddlUnit.DataTextField = "UnitNo";
                ddlUnit.DataValueField = "UnitId";
                ddlUnit.DataBind();
            }

            ddlUnit.Items.Insert(0, new ListItem("-- Select Unit --", "0"));
        }


        protected void ddlSociety_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlSociety.SelectedValue != "0")
                LoadBuildings(Convert.ToInt64(ddlSociety.SelectedValue));

            ddlUnit.Items.Clear();
            ddlUnit.Items.Insert(0, new ListItem("-- Select Unit --", "0"));
        }

        protected void ddlBuilding_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlBuilding.SelectedValue != "0")
                LoadUnits(Convert.ToInt64(ddlBuilding.SelectedValue));
            else
            {
                ddlUnit.Items.Clear();
                ddlUnit.Items.Insert(0, new ListItem("-- Select Unit --", "0"));
            }
        }

        #endregion

        #region Load Member for Editing

        private void LoadMemberData(long memberId)
        {
            string query = @"
                SELECT 
                    m.MemberId,
                    m.SocietyId,
                    m.FullName,
                    m.Email,
                    m.Phone,
                    m.Status,
                    u.UnitId,
                    u.UnitNo,
                    b.BuildingId,
                    b.Name AS BuildingName
                FROM Members m
                LEFT JOIN UnitOccupancies o ON m.MemberId = o.MemberId
                LEFT JOIN Units u ON o.UnitId = u.UnitId
                LEFT JOIN Buildings b ON u.BuildingId = b.BuildingId
                WHERE m.MemberId = @MemberId
                  AND (o.EndDate IS NULL OR o.EndDate > GETDATE());
            ";

            using (SqlConnection con = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@MemberId", memberId);
                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        ddlSociety.SelectedValue = dr["SocietyId"].ToString();

                        // Load dependent dropdowns
                        long societyId = Convert.ToInt64(dr["SocietyId"]);
                        LoadBuildings(societyId);

                        if (dr["BuildingId"] != DBNull.Value)
                        {
                            long buildingId = Convert.ToInt64(dr["BuildingId"]);
                            ddlBuilding.SelectedValue = buildingId.ToString();
                            LoadUnits(buildingId);
                        }

                        if (dr["UnitId"] != DBNull.Value)
                            ddlUnit.SelectedValue = dr["UnitId"].ToString();

                        txtFullName.Text = dr["FullName"].ToString();
                        txtEmail.Text = dr["Email"].ToString();
                        txtContact.Text = dr["Phone"].ToString();
                        ddlStatus.SelectedValue = dr["Status"].ToString();
                    }
                }
            }
        }

        #endregion

        #region Save Member (Add / Update)

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (ddlSociety.SelectedValue == "0" || ddlBuilding.SelectedValue == "0" || ddlUnit.SelectedValue == "0")
            {
                ShowMessage("⚠️ Please select Society, Building, and Unit.", false);
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();

                    // Prevent assigning an occupied unit
                    using (SqlCommand checkCmd = new SqlCommand(
                        "SELECT COUNT(*) FROM UnitOccupancies WHERE UnitId=@UnitId AND EndDate IS NULL", con))
                    {
                        checkCmd.Parameters.AddWithValue("@UnitId", ddlUnit.SelectedValue);
                        int occupiedCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (occupiedCount > 0 && Request.QueryString["MemberId"] == null)
                        {
                            ShowMessage("❌ This unit is already occupied by another member.", false);
                            return;
                        }
                    }

                    if (Request.QueryString["MemberId"] == null)
                        InsertMember(con);
                    else
                        UpdateMember(con);
                }
            }
            catch (Exception ex)
            {
                ShowMessage("❌ Error: " + ex.Message, false);
            }
        }

        private void InsertMember(SqlConnection con)
        {
            string insertQuery = @"
                INSERT INTO Members (SocietyId, FullName, Email, Phone, Status, CreatedAt)
                OUTPUT INSERTED.MemberId
                VALUES (@SocietyId, @FullName, @Email, @Phone, @Status, GETDATE());
            ";

            using (SqlCommand cmd = new SqlCommand(insertQuery, con))
            {
                cmd.Parameters.AddWithValue("@SocietyId", ddlSociety.SelectedValue);
                cmd.Parameters.AddWithValue("@FullName", txtFullName.Text.Trim());
                cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                cmd.Parameters.AddWithValue("@Phone", txtContact.Text.Trim());
                cmd.Parameters.AddWithValue("@Status", ddlStatus.SelectedValue);

                long newMemberId = Convert.ToInt64(cmd.ExecuteScalar());

                using (SqlCommand occCmd = new SqlCommand(
                    "INSERT INTO UnitOccupancies (UnitId, MemberId, Type, StartDate) VALUES (@UnitId, @MemberId, @Type, GETDATE())", con))
                {
                    occCmd.Parameters.AddWithValue("@UnitId", ddlUnit.SelectedValue);
                    occCmd.Parameters.AddWithValue("@MemberId", newMemberId);
                    occCmd.Parameters.AddWithValue("@Type", "Owner");
                    occCmd.ExecuteNonQuery();
                }

                ShowMessage("✅ Member added successfully!", true);
            }
        }

        private void UpdateMember(SqlConnection con)
        {
            long memberId = Convert.ToInt64(Request.QueryString["MemberId"]);

            using (SqlCommand cmd = new SqlCommand(@"
                UPDATE Members 
                SET FullName=@FullName, Email=@Email, Phone=@Phone, Status=@Status, SocietyId=@SocietyId 
                WHERE MemberId=@MemberId", con))
            {
                cmd.Parameters.AddWithValue("@FullName", txtFullName.Text.Trim());
                cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                cmd.Parameters.AddWithValue("@Phone", txtContact.Text.Trim());
                cmd.Parameters.AddWithValue("@Status", ddlStatus.SelectedValue);
                cmd.Parameters.AddWithValue("@SocietyId", ddlSociety.SelectedValue);
                cmd.Parameters.AddWithValue("@MemberId", memberId);
                cmd.ExecuteNonQuery();
            }

            ShowMessage("✅ Member updated successfully!", true);
        }

        #endregion

        #region Helpers

        private void ShowMessage(string message, bool success)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = success ? "text-success" : "text-danger";
        }

        #endregion
    }
}
