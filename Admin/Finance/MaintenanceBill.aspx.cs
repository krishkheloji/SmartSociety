using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SocietyManagement.Admin.Finance
{
    public partial class MaintenanceBill : System.Web.UI.Page
    {
        string cs = ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString;
        static DataTable billItems;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadSocieties();
                InitializeBillItems();
                LoadExistingBills();
            }
        }

        // 🔹 Initialize DataTable for Bill Items
        private void InitializeBillItems()
        {
            billItems = new DataTable();
            billItems.Columns.Add("Description");
            billItems.Columns.Add("Amount", typeof(decimal));
            billItems.Columns.Add("ExpenseId", typeof(int));
            gvBillItems.DataSource = billItems;
            gvBillItems.DataBind();
        }

        private void LoadSocieties()
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("SELECT SocietyId, Name FROM Societies", con);
                con.Open();
                ddlSociety.DataSource = cmd.ExecuteReader();
                ddlSociety.DataTextField = "Name";
                ddlSociety.DataValueField = "SocietyId";
                ddlSociety.DataBind();
            }
            ddlSociety.Items.Insert(0, new ListItem("-- Select Society --", ""));
        }

        protected void ddlSociety_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlUnit.Items.Clear();
            txtTotalAmount.Text = "0.00";

            if (string.IsNullOrEmpty(ddlSociety.SelectedValue))
                return;

            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand(@"
                    SELECT UnitId, UnitNo 
                    FROM Units 
                    WHERE BuildingId IN 
                    (SELECT BuildingId FROM Buildings WHERE SocietyId = @SocietyId)", con);

                cmd.Parameters.AddWithValue("@SocietyId", ddlSociety.SelectedValue);
                con.Open();
                ddlUnit.DataSource = cmd.ExecuteReader();
                ddlUnit.DataTextField = "UnitNo";
                ddlUnit.DataValueField = "UnitId";
                ddlUnit.DataBind();
            }

            ddlUnit.Items.Insert(0, new ListItem("-- Select Unit --", ""));
        }

        protected void ddlUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlSociety.SelectedValue) && !string.IsNullOrEmpty(ddlUnit.SelectedValue))
            {
                LoadBillItemsForUnit(Convert.ToInt32(ddlSociety.SelectedValue), Convert.ToInt32(ddlUnit.SelectedValue));
            }
        }

        // 🔹 Load all distributed expenses for this unit
        private void LoadBillItemsForUnit(int societyId, int unitId)
        {
            billItems.Rows.Clear();

            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand(@"
                    SELECT bi.Description, bi.Amount, bi.ExpenseId
                    FROM BillItems bi
                    INNER JOIN MaintenanceBills mb ON bi.BillId = mb.BillId
                    WHERE mb.SocietyId = @SocietyId AND mb.UnitId = @UnitId
                    ORDER BY bi.ItemId", con);

                cmd.Parameters.AddWithValue("@SocietyId", societyId);
                cmd.Parameters.AddWithValue("@UnitId", unitId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(billItems);
            }

            gvBillItems.DataSource = billItems;
            gvBillItems.DataBind();

            txtTotalAmount.Text = billItems.AsEnumerable().Sum(r => r.Field<decimal>("Amount")).ToString("0.00");
        }

        // 🔹 Add manual item (admin adds)
        protected void btnAddItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TextBox1.Text) && decimal.TryParse(TextBox2.Text, out decimal amt))
            {
                // keep manual items separate (ExpenseId = 0)
                billItems.Rows.Add(TextBox1.Text.Trim(), amt, 0);

                gvBillItems.DataSource = billItems;
                gvBillItems.DataBind();

                txtTotalAmount.Text = billItems.AsEnumerable().Sum(r => r.Field<decimal>("Amount")).ToString("0.00");

                TextBox1.Text = "";
                TextBox2.Text = "";
            }
        }

        // 🔹 Generate the full bill
        protected void btnGenerateBill_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ddlSociety.SelectedValue) || string.IsNullOrEmpty(ddlUnit.SelectedValue))
            {
                lblMessage.CssClass = "text-danger fw-bold";
                lblMessage.Text = "⚠️ Please select Society and Unit.";
                return;
            }

            long billId = 0;

            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                SqlTransaction tr = con.BeginTransaction();

                try
                {
                    // 1️⃣ Get BuildingId
                    SqlCommand cmdBuilding = new SqlCommand("SELECT BuildingId FROM Units WHERE UnitId = @UnitId", con, tr);
                    cmdBuilding.Parameters.AddWithValue("@UnitId", ddlUnit.SelectedValue);
                    int buildingId = Convert.ToInt32(cmdBuilding.ExecuteScalar());

                    // 2️⃣ Add distributable expenses
                    AddDistributableExpenses(con, tr, Convert.ToInt32(ddlSociety.SelectedValue), buildingId);

                    // 3️⃣ Create new maintenance bill
                    SqlCommand cmdBill = new SqlCommand(@"
                INSERT INTO MaintenanceBills (SocietyId, UnitId, BillMonth, DueDate, TotalAmount, Status)
                VALUES (@SocietyId, @UnitId, @BillMonth, @DueDate, @TotalAmount, 'Unpaid');
                SELECT SCOPE_IDENTITY();", con, tr);

                    cmdBill.Parameters.AddWithValue("@SocietyId", ddlSociety.SelectedValue);
                    cmdBill.Parameters.AddWithValue("@UnitId", ddlUnit.SelectedValue);
                    cmdBill.Parameters.AddWithValue("@BillMonth", Convert.ToDateTime(txtBillMonth.Text + "-01"));
                    cmdBill.Parameters.AddWithValue("@DueDate", txtDueDate.Text);

                    decimal total = billItems.AsEnumerable().Sum(r => r.Field<decimal>("Amount"));
                    cmdBill.Parameters.AddWithValue("@TotalAmount", total);

                    billId = Convert.ToInt64(cmdBill.ExecuteScalar());

                    // 4️⃣ Insert each bill item
                    foreach (DataRow row in billItems.Rows)
                    {
                        SqlCommand cmdItem = new SqlCommand(@"
                    INSERT INTO BillItems (BillId, Description, Amount, ExpenseId)
                    VALUES (@BillId, @Desc, @Amt, @ExpenseId)", con, tr);

                        cmdItem.Parameters.AddWithValue("@BillId", billId);
                        cmdItem.Parameters.AddWithValue("@Desc", row["Description"]);
                        cmdItem.Parameters.AddWithValue("@Amt", row["Amount"]);

                        // ✅ Extract ExpenseId if exists in Description
                        string desc = row["Description"].ToString();
                        int expenseId = 0;
                        if (desc.Contains("ExpID:"))
                        {
                            string[] parts = desc.Split(new[] { "ExpID:" }, StringSplitOptions.None);
                            if (parts.Length > 1)
                            {
                                string idPart = parts[1].Trim();
                                int.TryParse(new string(idPart.TakeWhile(char.IsDigit).ToArray()), out expenseId);
                            }
                        }

                        cmdItem.Parameters.AddWithValue("@ExpenseId", expenseId > 0 ? (object)expenseId : DBNull.Value);
                        cmdItem.ExecuteNonQuery();
                    }

                    tr.Commit(); // ✅ Only once
                    try

                    {

                        SendMaintenanceBillNotification(

                            Convert.ToInt64(ddlUnit.SelectedValue),

                            Convert.ToDateTime(txtBillMonth.Text + "-01")

                        );

                    }

                    catch (Exception notifEx)

                    {



                        System.Diagnostics.Debug.WriteLine("Notification error: " + notifEx.Message);

                    }





                    LoadExistingBills();

                    billItems.Rows.Clear();

                    gvBillItems.DataSource = null;

                    gvBillItems.DataBind();
                }
                catch (Exception ex)
                {
                    tr.Rollback();
                    lblMessage.CssClass = "text-danger fw-bold";
                    lblMessage.Text = "❌ Error: " + ex.Message;
                    return; // stop further execution
                }
            }

            // ✅ Now transaction is over; safe to call other functions
            try
            {
                SendBillGeneratedEmail(Convert.ToInt64(ddlUnit.SelectedValue), billId);
            }
            catch { /* ignore email failures */ }

            // ✅ Refresh and reset
            LoadExistingBills();
            billItems.Rows.Clear();
            gvBillItems.DataSource = null;
            gvBillItems.DataBind();
            txtTotalAmount.Text = "";
            TextBox1.Text = "";
            TextBox2.Text = "";
            ddlUnit.SelectedIndex = 0;
            txtBillMonth.Text = "";
            txtDueDate.Text = "";

            lblMessage.CssClass = "text-success fw-bold";
            lblMessage.Text = "✅ Maintenance bill generated successfully and email sent!";
        }


        // 🔹 Send email notification to member after bill generation
        private void SendBillGeneratedEmail(long unitId, long billId)
        {
            try
            {
                string memberEmail = "";
                string unitNo = "";
                decimal totalAmount = 0;
                DateTime billMonth = DateTime.Now;
                DateTime dueDate = DateTime.Now;

                using (SqlConnection con = new SqlConnection(cs))
                {
                    SqlCommand cmd = new SqlCommand(@"
               SELECT M.Email, U.UnitNo, MB.TotalAmount, MB.BillMonth, MB.DueDate
               FROM MaintenanceBills MB
               INNER JOIN Units U ON MB.UnitId = U.UnitId
               INNER JOIN UnitOccupancies O ON U.UnitId = O.UnitId
               INNER JOIN Members M ON O.MemberId = M.MemberId
                WHERE MB.BillId = @BillId;
                ", con);
                    cmd.Parameters.AddWithValue("@BillId", billId);
                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        memberEmail = dr["Email"].ToString();
                        unitNo = dr["UnitNo"].ToString();
                        totalAmount = Convert.ToDecimal(dr["TotalAmount"]);
                        billMonth = Convert.ToDateTime(dr["BillMonth"]);
                        dueDate = Convert.ToDateTime(dr["DueDate"]);
                    }
                    dr.Close();
                }



                lblMessage.Text = "Preparing to send mail...";

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("ajaychaugule2002@gmail.com", "Society Admin");
                mail.To.Add(memberEmail);
                mail.Subject = $"Maintenance Bill - {billMonth:MMMM yyyy}";
                mail.Body =
                    $"Dear Member,\n\n" +
                    $"Your maintenance bill for Unit No. {unitNo} has been generated.\n\n" +
                    $"Bill Month: {billMonth:MMMM yyyy}\n" +
                    $"Total Amount: ₹{totalAmount:0.00}\n" +
                    $"Due Date: {dueDate:dd-MMM-yyyy}\n\n" +
                    $"Thank you,\nSociety Management Team";
                mail.IsBodyHtml = false;

                SmtpClient smtpClient = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("ajaychaugule2002@gmail.com", "oxez vkdu aiky uebu"),
                    Timeout = 20000
                };

                // Debug logging
                lblMessage.Text += "<br/>Connecting to SMTP...";

                smtpClient.Send(mail);

                lblMessage.Text += "<br/>✅ Mail sent successfully!";
            }
            catch (SmtpException smtpEx)
            {
                lblMessage.Text = "❌ SMTP Error: " + smtpEx.Message;
            }
            catch (Exception ex)
            {
                lblMessage.Text = "❌ General Error: " + ex.Message;
            }
        }

        // 🔹 Add any distributable expenses (if not yet added)
        private void AddDistributableExpenses(SqlConnection con, SqlTransaction tr, int societyId, int buildingId)
        {
            try
            {
                SqlCommand cmd = new SqlCommand(@"
                    SELECT ExpenseId, Category, Amount, BuildingId
                    FROM Expenses
                    WHERE SocietyId = @SocietyId AND IsDistributable = 1
                      AND (BuildingId = @BuildingId OR BuildingId IS NULL)
                      AND ExpenseId NOT IN (SELECT ExpenseId FROM BillItems WHERE ExpenseId IS NOT NULL)", con, tr);

                cmd.Parameters.AddWithValue("@SocietyId", societyId);
                cmd.Parameters.AddWithValue("@BuildingId", buildingId);

                SqlDataReader reader = cmd.ExecuteReader();
                DataTable dt = new DataTable();
                dt.Load(reader);

                foreach (DataRow exp in dt.Rows)
                {
                    int expenseId = Convert.ToInt32(exp["ExpenseId"]);
                    string category = exp["Category"].ToString();
                    decimal totalAmount = Convert.ToDecimal(exp["Amount"]);

                    // count units for building/society
                    string queryCount = @"
                        SELECT COUNT(DISTINCT u.UnitId)
                        FROM Units u
                        INNER JOIN UnitOccupancies uo ON u.UnitId = uo.UnitId
                        WHERE (@BuildingId IS NULL OR u.BuildingId = @BuildingId)
                        AND uo.EndDate IS NULL";

                    SqlCommand cmdCount = new SqlCommand(queryCount, con, tr);
                    cmdCount.Parameters.AddWithValue("@BuildingId", exp["BuildingId"] == DBNull.Value ? (object)DBNull.Value : exp["BuildingId"]);
                    int count = Convert.ToInt32(cmdCount.ExecuteScalar());

                    if (count > 0)
                    {
                        decimal perShare = Math.Round(totalAmount / count, 2);
                        string desc = $"{category} (Shared) - ExpID:{expenseId}";
                        billItems.Rows.Add(desc, perShare, expenseId);
                    }
                }

                gvBillItems.DataSource = billItems;
                gvBillItems.DataBind();
                txtTotalAmount.Text = billItems.AsEnumerable().Sum(r => r.Field<decimal>("Amount")).ToString("0.00");
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding distributable expenses: " + ex.Message);
            }
        }

        private void LoadExistingBills()
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand(@"
                    SELECT MB.BillId, U.UnitNo, MB.BillMonth, MB.DueDate, MB.TotalAmount, MB.Status
                    FROM MaintenanceBills MB
                    INNER JOIN Units U ON MB.UnitId = U.UnitId
                    ORDER BY MB.BillId DESC", con);

                con.Open();
                gvBills.DataSource = cmd.ExecuteReader();
                gvBills.DataBind();
            }
        }

        private void SendMaintenanceBillNotification(long unitId, DateTime billMonth)

        {

            using (SqlConnection con = new SqlConnection(cs))

            {

                con.Open();





                SqlCommand cmdGetMember = new SqlCommand(@"

            SELECT TOP 1 MemberId 

            FROM UnitOccupancies 

            WHERE UnitId = @UnitId AND (EndDate IS NULL OR EndDate > GETDATE())", con);

                cmdGetMember.Parameters.AddWithValue("@UnitId", unitId);



                object memberIdObj = cmdGetMember.ExecuteScalar();

                if (memberIdObj == null) return;



                long memberId = Convert.ToInt64(memberIdObj);





                SqlCommand cmdGetUser = new SqlCommand("SELECT TOP 1 UserId FROM Users WHERE MemberId = @MemberId", con);

                cmdGetUser.Parameters.AddWithValue("@MemberId", memberId);



                object userIdObj = cmdGetUser.ExecuteScalar();

                if (userIdObj == null) return;



                long userId = Convert.ToInt64(userIdObj);





                SqlCommand cmdNotif = new SqlCommand(@"

            INSERT INTO Notifications (UserId, Title, Message, Link)

            VALUES (@UserId, @Title, @Message, @Link)", con);



                string monthName = billMonth.ToString("MMMM yyyy");



                cmdNotif.Parameters.AddWithValue("@UserId", userId);

                cmdNotif.Parameters.AddWithValue("@Title", "New Maintenance Bill Generated");

                cmdNotif.Parameters.AddWithValue("@Message", $"Your maintenance bill for {monthName} has been generated.");

                cmdNotif.Parameters.AddWithValue("@Link", "~/Member/Maintenance/ViewBills.aspx");



                cmdNotif.ExecuteNonQuery();

            }

        }

    }
}
