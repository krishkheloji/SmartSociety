using Razorpay.Api;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SocietyManagement.Member.Maintenance
{
    public partial class MakePayment : System.Web.UI.Page
    {
        string cs = ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString["BillId"] != null)
                {
                    int billId = Convert.ToInt32(Request.QueryString["BillId"]);
                    LoadBillDetails(billId);
                }
                else
                {
                    lblMessage.Text = "Invalid request.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
            }
        }

        private void LoadBillDetails(int billId)
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = @"SELECT BillId, FORMAT(BillMonth,'MMM yyyy') AS BillMonth, 
                                 TotalAmount, DueDate 
                                 FROM MaintenanceBills WHERE BillId = @BillId";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@BillId", billId);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    lblBillId.Text = dr["BillId"].ToString();
                    lblMonth.Text = dr["BillMonth"].ToString();
                    lblAmount.Text = Convert.ToDecimal(dr["TotalAmount"]).ToString("N2");
                    lblDueDate.Text = Convert.ToDateTime(dr["DueDate"]).ToString("dd-MM-yyyy");
                    pnlBill.Visible = true;
                }
                else
                {
                    lblMessage.Text = "Bill not found.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
                con.Close();
            }
        }

        protected void btnPay_Click(object sender, EventArgs e)
        {
            if (Request.QueryString["BillId"] == null)
            {
                lblMessage.Text = "Invalid payment request.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            int billId = Convert.ToInt32(Request.QueryString["BillId"]);
            decimal amount = Convert.ToDecimal(lblAmount.Text);

            ProcessRazorpayPayment(amount, billId);
        }

        private void ProcessRazorpayPayment(decimal amount, int billId)
        {
            // ✅ Razorpay test credentials
            string keyId = "rzp_test_Kl7588Yie2yJTV";
            string keySecret = "6dN9Nqs7M6HPFMlL45AhaTgp";

            RazorpayClient client = new RazorpayClient(keyId, keySecret);

            var options = new Dictionary<string, object>
            {
                { "amount", amount * 100 },  // Razorpay expects amount in paise
                { "currency", "INR" },
                { "receipt", "maintbill_rcpt_" + billId },
                { "payment_capture", 1 }
            };

            var order = client.Order.Create(options);
            string orderId = order["id"].ToString();

            // ✅ Razorpay payment popup script
            string razorpayScript = $@"
                <script src='https://checkout.razorpay.com/v1/checkout.js'></script>
                <script>
                    var options = {{
                        'key': '{keyId}',
                        'amount': {amount * 100},
                        'currency': 'INR',
                        'name': 'Society Management',
                        'description': 'Maintenance Bill Payment',
                        'order_id': '{orderId}',
                        'handler': function (response) {{
                            window.location.href = 'PaymentSuccess.aspx?payment_id=' + response.razorpay_payment_id + '&bill_id={billId}';
                        }},
                        'prefill': {{
                            'name': 'Test Member',
                            'email': 'testuser@example.com',
                            'contact': '9876543210'
                        }},
                        'theme': {{
                            'color': '#007bff'
                        }}
                    }};
                    var rzp1 = new Razorpay(options);
                    rzp1.open();
                </script>";

            ClientScript.RegisterStartupScript(this.GetType(), "razorpayScript", razorpayScript);
        }

    }
}