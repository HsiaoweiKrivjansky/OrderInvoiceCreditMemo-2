using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mongoose.IDO;
using Mongoose.IDO.Protocol;
using Mongoose.IDO.DataAccess;
using Mongoose.Core.Common;
using System.Data;
using Mongoose.WinStudio.Enums;

namespace ue_Mack_SLOrderInvoicingCreditMemoReport
{
    [IDOExtensionClass("ue_Mack_SLOrderInvoicingCreditMemoReport")]
    public class ue_Mack_SLOrderInvoicingCreditMemoReport : IDOExtensionClass
    {
        public override void SetContext(IIDOExtensionClassContext context)
        {
            // Call the base class implementation:    
            base.SetContext(context);
            // Add event handlers here, for example:    
            this.Context.IDO.PostLoadCollection += this.HandlePostLoadCollection;

        }

        private void HandlePostLoadCollection(object sender, IDOEventArgs args)
        {
            // Get the original RequestPayload;
            LoadCollectionRequestData originalRequest = (LoadCollectionRequestData)args.RequestPayload;

            // Get the original ResponsePayload:
            LoadCollectionResponseData originalResponse = (LoadCollectionResponseData)args.ResponsePayload;


            int idxCoNum = originalResponse.PropertyList.IndexOf("CoNum");
            int idxTxType = originalResponse.PropertyList.IndexOf("TxType");
            int idxStrCoLine = originalResponse.PropertyList.IndexOf("StrCoLine");
            int idxInvNum = originalResponse.PropertyList.IndexOf("InvNum");
            int idxInvTaxCode = originalResponse.PropertyList.IndexOf("InvTaxCode");
            
            int idxue_Structure = originalResponse.PropertyList.IndexOf("ue_Structure");
            int idxue_SpecID = originalResponse.PropertyList.IndexOf("ue_SpecID");
            int idxShipmentId = originalResponse.PropertyList.IndexOf("ShipmentId");
            int idxue_JobName = originalResponse.PropertyList.IndexOf("ue_JobName");
            int idxue_CustPo = originalResponse.PropertyList.IndexOf("ue_CustPo");
            int idxue_Infobar = originalResponse.PropertyList.IndexOf("ue_Infobar");
            int idxue_ItemDesc = originalResponse.PropertyList.IndexOf("ItemDesc");
            

            //string xxx = originalResponse.PropertyList.Count.ToString();

            foreach (IDOItem item in originalResponse.Items)
            {
                //Keys
                //string CoitemRowPointer = "";
                string CoNum = "";
                string TxType = "";
                string StrCoLine = "";
                string CoLine = "";
                string CoRelease = "";
                string InvNum = "";

                CoNum = item.PropertyValues[idxCoNum].Value.ToString();
                TxType = item.PropertyValues[idxTxType].Value.ToString();
                StrCoLine = item.PropertyValues[idxStrCoLine].Value.ToString();
                InvNum = item.PropertyValues[idxInvNum].Value.ToString();

                //output Values
                string vStructure = "";
                string vSpecID = "";
                string vJobName = "";
                string vCustPo = "";
                string vShipmentId = "";
                string Infobar = "";
                string vDescription = "";
                string vInvTaxCode = "";

                LoadCollectionRequestData request = new LoadCollectionRequestData();
                LoadCollectionResponseData response = new LoadCollectionResponseData();

                // get Job Name
                try
                {
                    request.IDOName = "SLCos";
                    request.PropertyList.SetProperties("coUf_JobProjName, CustPo");
                    request.Filter = string.Format("CoNum = {0}", SqlLiteral.Format(CoNum));
                    request.RecordCap = 1;
                    response = this.Context.Commands.LoadCollection(request);

                    vJobName = response[0, "coUf_JobProjName"].Value;
                    vCustPo = response[0, "CustPo"].Value;
                }
                catch (Exception exc)
                {
                    Infobar = "SLCos" + " :::  " + exc.ToString();
                }

                // get structure
                if (TxType == "3")
                {
                    try
                    {
                        CoLine = StrCoLine.Substring(0, 4).Trim();
                        CoRelease = StrCoLine.Substring(6, 4).Trim();
                        if (CoRelease == "")
                            CoRelease = "0";

                        request.IDOName = "SLCoitems";
                        request.PropertyList.SetProperties("coiUf_Structure,coiUf_SpecCoLineID,Description");
                        request.Filter = string.Format("CoNum = {0} and CoLine = {1} and CoRelease = {2}"
                            , SqlLiteral.Format(CoNum), CoLine, CoRelease);
                        request.RecordCap = 1;
                        response = this.Context.Commands.LoadCollection(request);

                        vStructure = response[0, "coiUf_Structure"].Value;
                        vSpecID = response[0, "coiUf_SpecCoLineID"].Value;
                        vDescription = response[0, "Description"].Value;
                    }
                    catch (Exception exc)
                    {
                        Infobar = "SLCoitems" + " :::  " + exc.ToString();
                    }
                }

                // get Shipment
                if (TxType == "3")
                {
                    try
                    {
                        CoLine = StrCoLine.Substring(0, 4).Trim();
                        CoRelease = StrCoLine.Substring(6, 4).Trim();
                        if (CoRelease == "")
                            CoRelease = "0";

                        request.IDOName = "SLShipmentLines";
                        request.PropertyList.SetProperties("ShipmentId");
                        request.Filter = string.Format("RefNum = {0} and RefLineSuf = {1} and RefRelease = {2}"
                            , SqlLiteral.Format(CoNum), CoLine, CoRelease);
                        request.RecordCap = 1;
                        response = this.Context.Commands.LoadCollection(request);

                        if (response.Items.Count != 0)
                            vShipmentId = response[0, "ShipmentId"].Value;
                    }
                    catch (Exception exc)
                    {
                        Infobar = "SLShipmentLines" + " :::  " + exc.ToString();
                    }
                }

                // get Tax Code
                if (TxType == "1")
                {
                    try
                    {
                        request.IDOName = "SLInvHdrs";
                        request.PropertyList.SetProperties("TaxCode1");
                        request.Filter = string.Format("InvNum = {0}"
                            , SqlLiteral.Format(InvNum));
                        request.RecordCap = 1;
                        response = this.Context.Commands.LoadCollection(request);

                        if (response.Items.Count != 0)
                            vInvTaxCode = response[0, "TaxCode1"].Value;
                    }
                    catch (Exception exc)
                    {
                        Infobar = "SLInvHdrs" + " :::  " + exc.ToString();
                    }
                }
                item.PropertyValues[idxue_Structure].SetValue(vStructure);
                item.PropertyValues[idxShipmentId].SetValue(vShipmentId);
                item.PropertyValues[idxue_JobName].SetValue(vJobName);
                item.PropertyValues[idxue_CustPo].SetValue(vCustPo);
                item.PropertyValues[idxue_Infobar].SetValue(Infobar);
                item.PropertyValues[idxue_SpecID].SetValue(vSpecID);
                item.PropertyValues[idxue_ItemDesc].SetValue(vDescription);
                item.PropertyValues[idxInvTaxCode].SetValue(idxInvTaxCode);
            }
        }
    }
}

