using Razorpay.Api;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;

namespace SocietyManagement.Member.LightBill
{
    public partial class PayLightBill : System.Web.UI.Page
    {
        string cs = ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString["ReadingId"] != null)
                {
                    LoadBillDetails(Convert.ToInt64(Request.QueryString["ReadingId"]));
                }
                else
                {
                    lblMsg.Text = "Invalid access! No reading selected.";
                }
            }
        }

        private void LoadBillDetails(long readingId)
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = @"SELECT ReadingDate, ReadingValue, RatePerUnit,
                                (ReadingValue * RatePerUnit) AS TotalAmount
                                FROM MeterReadings
                                WHERE ReadingId = @ReadingId";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ReadingId", readingId);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    lblDate.Text = Convert.ToDateTime(dr["ReadingDate"]).ToString("dd-MM-yyyy");
                    lblUnits.Text = dr["ReadingValue"].ToString();
                    lblRate.Text = dr["RatePerUnit"].ToString();
                    lblTotal.Text = Convert.ToDecimal(dr["TotalAmount"]).ToString("N2");
                    pnlBill.Visible = true;
                }
                else
                {
                    lblMsg.Text = "No bill found for this reading.";
                }
                con.Close();
            }
        }

        protected void btnPayNow_Click(object sender, EventArgs e)
        {
            if (Request.QueryString["ReadingId"] == null)
            {
                lblMsg.Text = "Invalid payment request.";
                return;
            }

            long readingId = Convert.ToInt64(Request.QueryString["ReadingId"]);
            decimal amount = Convert.ToDecimal(lblTotal.Text);

            ProcessRazorpayPayment(amount, readingId);
        }

        private void ProcessRazorpayPayment(decimal amount, long readingId)
        {
            // ✅ Razorpay test credentials (replace with live keys later)
            string keyId = "rzp_test_Kl7588Yie2yJTV";
            string keySecret = "6dN9Nqs7M6HPFMlL45AhaTgp";

            RazorpayClient client = new RazorpayClient(keyId, keySecret);

            var options = new Dictionary<string, object>
            {
                { "amount", amount * 100 },  // Amount in paise
                { "currency", "INR" },
                { "receipt", "lightbill_rcpt_" + readingId },
                { "payment_capture", 1 }
            };

            var order = client.Order.Create(options);
            string orderId = order["id"].ToString();

            // ✅ Generate Razorpay payment script dynamically
            string razorpayScript = $@"
                <script src='https://checkout.razorpay.com/v1/checkout.js'></script>
                <script>
                    var options = {{
                        'key': '{keyId}',
                        'amount': {amount * 100},
                        'currency': 'INR',
                        'name': 'Society Management',
                        'description': 'Light Bill Payment',
                        'order_id': '{orderId}',
                        'handler': function (response) {{
                            window.location.href = 'LightBillPaymentSuccess.aspx?payment_id=' + response.razorpay_payment_id + '&reading_id={readingId}';
                          }},
                        'prefill': {{
                                        'name': 'TestTeam2',
                                        'email': 'khelojikrish@gmail.com',
                                        'contact': '7208921898'
                                    }},
                        'theme': {{
                            'color': '#3399cc'
                        }}
                    }};
                    var rzp1 = new Razorpay(options);
                    rzp1.open();
                </script>";

            ClientScript.RegisterStartupScript(this.GetType(), "razorpayScript", razorpayScript);
        }
    }
}
