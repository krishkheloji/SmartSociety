using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SocietyManagement.Admin.Expenses
{
    public partial class ManageExpenses : System.Web.UI.Page
    {
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            ValidationSettings.UnobtrusiveValidationMode = UnobtrusiveValidationMode.None;

            if (!IsPostBack)
            {
                LoadSocieties();
                LoadVendors();
                LoadExpenses();
                txtExpenseDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
            }
        }

        // 🔹 Load societies
        private void LoadSocieties()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = "SELECT SocietyId, Name FROM Societies ORDER BY Name";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    ddlSociety.DataSource = dt;
                    ddlSociety.DataTextField = "Name";
                    ddlSociety.DataValueField = "SocietyId";
                    ddlSociety.DataBind();
                    ddlSociety.Items.Insert(0, new ListItem("-- Select Society --", "0"));
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading societies: " + ex.Message, false);
            }
        }

        protected void ddlSociety_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlSociety.SelectedValue != "0")
                LoadBuildings(Convert.ToInt32(ddlSociety.SelectedValue));
            else
            {
                ddlBuilding.Items.Clear();
                ddlBuilding.Items.Insert(0, new ListItem("-- Select Building --", "0"));
            }
        }

        private void LoadBuildings(int societyId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = "SELECT BuildingId, Name FROM Buildings WHERE SocietyId = @SocietyId ORDER BY Name";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@SocietyId", societyId);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    ddlBuilding.DataSource = dt;
                    ddlBuilding.DataTextField = "Name";
                    ddlBuilding.DataValueField = "BuildingId";
                    ddlBuilding.DataBind();
                    ddlBuilding.Items.Insert(0, new ListItem("-- All Buildings --", "0"));
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading buildings: " + ex.Message, false);
            }
        }

        private void LoadVendors()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = "SELECT VendorId, Name FROM Vendors ORDER BY Name";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    ddlVendor.DataSource = dt;
                    ddlVendor.DataTextField = "Name";
                    ddlVendor.DataValueField = "VendorId";
                    ddlVendor.DataBind();
                    ddlVendor.Items.Insert(0, new ListItem("-- No Vendor --", "0"));
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading vendors: " + ex.Message, false);
            }
        }

        // 🔹 Save expense
        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                int societyId = Convert.ToInt32(ddlSociety.SelectedValue);
                int buildingId = Convert.ToInt32(ddlBuilding.SelectedValue);
                int vendorId = Convert.ToInt32(ddlVendor.SelectedValue);
                string category = txtCategory.Text.Trim();
                decimal amount = Convert.ToDecimal(txtAmount.Text);
                DateTime expenseDate = Convert.ToDateTime(txtExpenseDate.Text);
                string notes = txtNotes.Text.Trim();
                bool isDistributable = chkDistributable.Checked;

                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    conn.Open();

                    // 🔸 Insert expense
                    string query = @"INSERT INTO Expenses (SocietyId, VendorId, ExpenseDate, Category, Amount, Notes, BuildingId, IsDistributable) 
                                     VALUES (@SocietyId, @VendorId, @ExpenseDate, @Category, @Amount, @Notes, @BuildingId, @IsDistributable);
                                     SELECT SCOPE_IDENTITY();";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@SocietyId", societyId);
                    cmd.Parameters.AddWithValue("@VendorId", vendorId == 0 ? (object)DBNull.Value : vendorId);
                    cmd.Parameters.AddWithValue("@ExpenseDate", expenseDate);
                    cmd.Parameters.AddWithValue("@Category", category);
                    cmd.Parameters.AddWithValue("@Amount", amount);
                    cmd.Parameters.AddWithValue("@Notes", string.IsNullOrEmpty(notes) ? (object)DBNull.Value : notes);
                    cmd.Parameters.AddWithValue("@BuildingId", buildingId == 0 ? (object)DBNull.Value : buildingId);
                    cmd.Parameters.AddWithValue("@IsDistributable", isDistributable);

                    int expenseId = Convert.ToInt32(cmd.ExecuteScalar());

                    // 🔹 If distributable, distribute immediately
                    if (isDistributable)
                    {
                        DistributeExpenseToUnits(societyId, buildingId == 0 ? (int?)null : buildingId, expenseId, category, amount);
                        ShowMessage("✅ Expense added and distributed successfully to all active units!", true);
                    }
                    else
                    {
                        ShowMessage("✅ Expense added successfully!", true);
                    }

                    ClearForm();
                    LoadExpenses();
                }
            }
            catch (Exception ex)
            {
                ShowMessage("❌ Error: " + ex.Message, false);
            }
        }

        private void LoadExpenses()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = @"SELECT 
                                        e.ExpenseId,
                                        e.ExpenseDate,
                                        e.Category,
                                        e.Amount,
                                        s.Name AS SocietyName,
                                        b.Name AS BuildingName,
                                        v.Name AS VendorName,
                                        e.Notes,
                                        CASE WHEN e.IsDistributable = 1 THEN 'Yes' ELSE 'No' END AS IsDistributable
                                     FROM Expenses e
                                     INNER JOIN Societies s ON e.SocietyId = s.SocietyId
                                     LEFT JOIN Buildings b ON e.BuildingId = b.BuildingId
                                     LEFT JOIN Vendors v ON e.VendorId = v.VendorId
                                     ORDER BY e.ExpenseDate DESC";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    gvExpenses.DataSource = dt;
                    gvExpenses.DataBind();
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading expenses: " + ex.Message, false);
            }
        }

        // Handle GridView commands (like Delete)
        protected void gvExpenses_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DeleteExpense")
            {
                int expenseId = Convert.ToInt32(e.CommandArgument);
                DeleteExpense(expenseId);
            }
        }

        // Delete selected expense
        private void DeleteExpense(int expenseId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = "DELETE FROM Expenses WHERE ExpenseId = @ExpenseId";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@ExpenseId", expenseId);

                    conn.Open();
                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        ShowMessage("✅ Expense deleted successfully!", true);
                        LoadExpenses();
                    }
                    else
                    {
                        ShowMessage("❌ Failed to delete expense.", false);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("❌ Error deleting expense: " + ex.Message, false);
            }
        }

        private void ShowMessage(string message, bool isSuccess)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = isSuccess ? "alert-custom alert alert-success" : "alert-custom alert alert-danger";
            lblMessage.Visible = true;
        }

        private void ClearForm()
        {
            ddlSociety.SelectedIndex = 0;
            ddlBuilding.Items.Clear();
            ddlBuilding.Items.Insert(0, new ListItem("-- Select Building --", "0"));
            txtCategory.Text = "";
            txtAmount.Text = "";
            ddlVendor.SelectedIndex = 0;
            txtNotes.Text = "";
            chkDistributable.Checked = false;
            txtExpenseDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
            lblMessage.Visible = false;
        }

        // 🔹 Distribute expense among all active units
        private void DistributeExpenseToUnits(int societyId, int? buildingId, int expenseId, string category, decimal totalAmount)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(GetConnectionString()))
                {
                    con.Open();
                    SqlTransaction tr = con.BeginTransaction();

                    try
                    {
                        string queryUnits = @"
                            SELECT DISTINCT u.UnitId
                            FROM Units u
                            INNER JOIN Buildings b ON u.BuildingId = b.BuildingId
                            INNER JOIN UnitOccupancies uo ON u.UnitId = uo.UnitId
                            WHERE b.SocietyId = @SocietyId 
                              AND uo.EndDate IS NULL
                              AND (@BuildingId IS NULL OR u.BuildingId = @BuildingId)";

                        SqlCommand cmdUnits = new SqlCommand(queryUnits, con, tr);
                        cmdUnits.Parameters.AddWithValue("@SocietyId", societyId);
                        cmdUnits.Parameters.AddWithValue("@BuildingId", (object)buildingId ?? DBNull.Value);

                        DataTable dtUnits = new DataTable();
                        dtUnits.Load(cmdUnits.ExecuteReader());

                        if (dtUnits.Rows.Count == 0)
                        {
                            tr.Commit();
                            return;
                        }

                        decimal perUnitShare = Math.Round(totalAmount / dtUnits.Rows.Count, 2);

                        foreach (DataRow unit in dtUnits.Rows)
                        {
                            long unitId = Convert.ToInt64(unit["UnitId"]);

                            SqlCommand cmdFindBill = new SqlCommand(@"
                                SELECT TOP 1 BillId 
                                FROM MaintenanceBills 
                                WHERE UnitId = @UnitId
                                  AND MONTH(BillMonth) = MONTH(GETDATE())
                                  AND YEAR(BillMonth) = YEAR(GETDATE())
                                  AND Status = 'Unpaid'
                                ORDER BY BillId DESC", con, tr);
                            cmdFindBill.Parameters.AddWithValue("@UnitId", unitId);
                            object billIdObj = cmdFindBill.ExecuteScalar();

                            long billId;
                            if (billIdObj == null)
                            {
                                SqlCommand cmdNewBill = new SqlCommand(@"
                                    INSERT INTO MaintenanceBills (SocietyId, UnitId, BillMonth, DueDate, TotalAmount, Status)
                                    VALUES (@SocietyId, @UnitId, GETDATE(), DATEADD(DAY, 10, GETDATE()), 0, 'Unpaid');
                                    SELECT SCOPE_IDENTITY();", con, tr);
                                cmdNewBill.Parameters.AddWithValue("@SocietyId", societyId);
                                cmdNewBill.Parameters.AddWithValue("@UnitId", unitId);
                                billId = Convert.ToInt64(cmdNewBill.ExecuteScalar());
                            }
                            else
                            {
                                billId = Convert.ToInt64(billIdObj);
                            }

                            // ✅ Include ExpenseId here
                            SqlCommand cmdItem = new SqlCommand(@"
                                INSERT INTO BillItems (BillId, Description, Amount, ExpenseId)
                                VALUES (@BillId, @Desc, @Amt, @ExpenseId)", con, tr);
                            cmdItem.Parameters.AddWithValue("@BillId", billId);
                            cmdItem.Parameters.AddWithValue("@Desc", $"{category} (Shared) - ExpID:{expenseId}");
                            cmdItem.Parameters.AddWithValue("@Amt", perUnitShare);
                            cmdItem.Parameters.AddWithValue("@ExpenseId", expenseId);
                            cmdItem.ExecuteNonQuery();

                            SqlCommand cmdUpdate = new SqlCommand(@"
                                UPDATE MaintenanceBills
                                SET TotalAmount = ISNULL(TotalAmount, 0) + @Amt
                                WHERE BillId = @BillId", con, tr);
                            cmdUpdate.Parameters.AddWithValue("@Amt", perUnitShare);
                            cmdUpdate.Parameters.AddWithValue("@BillId", billId);
                            cmdUpdate.ExecuteNonQuery();
                        }

                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        throw new Exception("Error distributing expense: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("⚠️ Failed to distribute expense: " + ex.Message, false);
            }
        }
    }
}
