using System;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Profile.Invoice
{
    public partial class Invoice_List : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                MoneyReceiptSQL.SelectCommand = @"
                    SELECT r.InvoiceReceiptID, r.InvoiceReceipt_SN, r.TotalAmount, r.PaidDate,
                           r.PaymentBy, r.PaidByUser, r.Collected_By, r.Payment_Method, r.SchoolID,
                           ISNULL(op.GatewayCharge, 0) AS GatewayCharge,
                           ISNULL(op.CustomerPaid,  0) AS CustomerPaid
                    FROM AAP_Invoice_Receipt r
                    LEFT JOIN (
                        SELECT
                            CAST(SUBSTRING(SP_Message,
                                CHARINDEX('ReceiptID:', SP_Message) + LEN('ReceiptID:'),
                                CHARINDEX(' |', SP_Message + ' |', CHARINDEX('ReceiptID:', SP_Message)) -
                                CHARINDEX('ReceiptID:', SP_Message) - LEN('ReceiptID:')
                            ) AS INT) AS ReceiptID,
                            CASE WHEN CHARINDEX('GatewayCharge:', SP_Message) > 0
                                 THEN TRY_CAST(LTRIM(RTRIM(SUBSTRING(SP_Message,
                                         CHARINDEX('GatewayCharge:', SP_Message) + LEN('GatewayCharge:'),
                                         CHARINDEX(' |', SP_Message + ' |', CHARINDEX('GatewayCharge:', SP_Message))
                                         - CHARINDEX('GatewayCharge:', SP_Message) - LEN('GatewayCharge:')
                                      ))) AS DECIMAL(18,2))
                                 ELSE 0 END AS GatewayCharge,
                            CASE WHEN CHARINDEX('CustomerPaid:', SP_Message) > 0
                                 THEN TRY_CAST(LTRIM(RTRIM(SUBSTRING(SP_Message,
                                         CHARINDEX('CustomerPaid:', SP_Message) + LEN('CustomerPaid:'),
                                         CHARINDEX(' |', SP_Message + ' |', CHARINDEX('CustomerPaid:', SP_Message))
                                         - CHARINDEX('CustomerPaid:', SP_Message) - LEN('CustomerPaid:')
                                      ))) AS DECIMAL(18,2))
                                 ELSE Amount END AS CustomerPaid
                        FROM AAP_Invoice_OnlinePayment
                        WHERE SP_Message LIKE 'ReceiptID:%'
                    ) op ON r.InvoiceReceiptID = op.ReceiptID
                    WHERE r.SchoolID = @SchoolID
                    ORDER BY r.InvoiceReceiptID DESC";
            }
        }
    }
}