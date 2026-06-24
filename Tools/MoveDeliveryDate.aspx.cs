using System;
using System.Web.UI;
using TrackerSQL.Classes;
using TrackerSQL.Managers;
using TrackerSQL.Repositories;

namespace TrackerSQL.Tools
{
    public partial class MoveDeliveryDate : Page
    {
        private readonly NextPrepDateByAreaRepository _repository = new NextPrepDateByAreaRepository();
        private readonly NextPrepDateDataSource _dataSource = new NextPrepDateDataSource();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                NewDeliveryDateTextBox.Text = string.Format("{0:d}", TimeZoneUtils.Now().AddDays(1).Date);
                BindPrepGrid();
            }
        }

        protected void btnMove_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime oldDate = Convert.ToDateTime(OldDeliveryDateDDL.SelectedValue);
                DateTime newDate = Convert.ToDateTime(NewDeliveryDateTextBox.Text);

                if (oldDate == DateTime.MinValue || newDate == DateTime.MinValue)
                {
                    StatusLiteral.Text = "New and old dates must be valid";
                    return;
                }

                int numRecs = _repository.MoveDeliveryDate(oldDate, newDate);
                StatusLiteral.Text = numRecs >= 0
                    ? $"Move done: {numRecs} record(s) updated."
                    : "ERROR: Failed to move delivery dates.";

                if (numRecs >= 0)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.System,
                        $"MoveDeliveryDate: Moved delivery dates from {oldDate:yyyy-MM-dd} to {newDate:yyyy-MM-dd}. {numRecs} records updated.");
                }

                BindPrepGrid();
                OldDeliveryDateDDL.DataBind();
            }
            catch (Exception ex)
            {
                StatusLiteral.Text = $"ERROR: {ex.Message}";
                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    $"MoveDeliveryDate error: {ex.Message}");
            }
        }

        private void BindPrepGrid()
        {
            gvPrepData.DataSource = _dataSource.GetAreaPrepDateGrid();
            gvPrepData.DataBind();
        }
    }
}
