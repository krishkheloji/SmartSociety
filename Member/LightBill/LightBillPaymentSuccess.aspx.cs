using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SocietyManagement.Member.LightBill
{
    public partial class LightBillPaymentSuccess : System.Web.UI.Page
    {
        string cs = ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string paymentId = Request.QueryString["payment_id"];
                string readingIdStr = Request.QueryString["reading_id"];

                if (!string.IsNullOrEmpty(paymentId) && !string.IsNullOrEmpty(readingIdStr))
                {
                    long readingId;
                    if (long.TryParse(readingIdStr, out readingId))
                    {
                        SaveLightBillPayment(paymentId, readingId);
                    }
                    else
                    {
                        lblMsg.Text = "❌ Invalid reading ID.";
                    }
                }
                else
                {
                    lblMsg.Text = "❌ Invalid payment confirmation.";
                }
            }
        }

        private void SaveLightBillPayment(string paymentId, long readingId)
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();

                // Step 1️⃣: Get reading and related details
                string q1 = @"SELECT M.UnitId, U.BuildingId, B.SocietyId, M.ReadingValue, M.RatePerUnit
                              FROM MeterReadings M
                              INNER JOIN Units U ON M.UnitId = U.UnitId
                              INNER JOIN Buildings B ON U.BuildingId = B.BuildingId
                              WHERE M.ReadingId = @ReadingId";

                SqlCommand cmd1 = new SqlCommand(q1, con);
                cmd1.Parameters.AddWithValue("@ReadingId", readingId);

                long unitId = 0, societyId = 0;
                decimal readingValue = 0, ratePerUnit = 0;

                using (SqlDataReader dr = cmd1.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        unitId = Convert.ToInt64(dr["UnitId"]);
                        societyId = Convert.ToInt64(dr["SocietyId"]);
                        readingValue = Convert.ToDecimal(dr["ReadingValue"]);
                        ratePerUnit = Convert.ToDecimal(dr["RatePerUnit"]);
                    }
                    else
                    {
                        lblMsg.ForeColor = System.Drawing.Color.Red;
                        lblMsg.Text = "❌ Reading not found.";
                        return;
                    }
                }

                decimal amount = readingValue * ratePerUnit;

                // Step 2️⃣: Insert into MaintenanceBills
                string insertBill = @"
                    INSERT INTO MaintenanceBills (SocietyId, UnitId, BillMonth, DueDate, TotalAmount, Status)
                    VALUES (@SocietyId, @UnitId, GETDATE(), DATEADD(DAY, 15, GETDATE()), @Amount, 'Paid');
                    SELECT SCOPE_IDENTITY();";

                SqlCommand cmdBill = new SqlCommand(insertBill, con);
                cmdBill.Parameters.AddWithValue("@SocietyId", societyId);
                cmdBill.Parameters.AddWithValue("@UnitId", unitId);
                cmdBill.Parameters.AddWithValue("@Amount", amount);

                long billId = Convert.ToInt64(cmdBill.ExecuteScalar());

                // Step 3️⃣: Insert Bill Item
                string insertItem = @"
                    INSERT INTO BillItems (BillId, Description, Amount)
                    VALUES (@BillId, @Description, @Amount)";

                SqlCommand cmdItem = new SqlCommand(insertItem, con);
                cmdItem.Parameters.AddWithValue("@BillId", billId);
                cmdItem.Parameters.AddWithValue("@Description", "Light Bill - " + DateTime.Now.ToString("MMMM yyyy"));
                cmdItem.Parameters.AddWithValue("@Amount", amount);
                cmdItem.ExecuteNonQuery();

                // Step 4️⃣: Insert Payment
                string insertPayment = @"
                    INSERT INTO Payments (BillId, PaidOn, Amount, Mode, ReferenceNo)
                    VALUES (@BillId, GETDATE(), @Amount, 'Razorpay', @ReferenceNo)";

                SqlCommand cmdPay = new SqlCommand(insertPayment, con);
                cmdPay.Parameters.AddWithValue("@BillId", billId);
                cmdPay.Parameters.AddWithValue("@Amount", amount);
                cmdPay.Parameters.AddWithValue("@ReferenceNo", "LR-" + readingId + "-" + paymentId);

                cmdPay.ExecuteNonQuery();

                con.Close();

                lblMsg.ForeColor = System.Drawing.Color.Green;
                lblMsg.Text = $"✅ Light Bill payment successful!<br/>" +
                              $"<b>Payment ID:</b> {paymentId}<br/>" +
                              $"<b>Amount:</b> ₹{amount:N2}";
            }
        }
    }
}