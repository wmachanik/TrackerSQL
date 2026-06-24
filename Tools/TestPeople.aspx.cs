using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Tools
{
    public partial class TestPeople : Page
    {
        private const string CONST_SORTEXPRESSION_VIEWSTATE = "PeopleSortExpression";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ViewState[CONST_SORTEXPRESSION_VIEWSTATE] = "Abbreviation";
                BindPeopleGrid();
            }
        }

        private void BindPeopleGrid()
        {
            try
            {
                var repo = new PersonsRepository();
                string sortBy = ViewState[CONST_SORTEXPRESSION_VIEWSTATE] as string ?? "Abbreviation";
                var people = repo.GetAll(sortBy);
                gvPeople.DataSource = people;
                gvPeople.DataBind();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "TestPeople BindPeopleGrid error: " + ex.Message);
            }
        }

        protected void gvPeople_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvPeople.PageIndex = e.NewPageIndex;
            BindPeopleGrid();
        }

        protected void gvPeople_Sorting(object sender, GridViewSortEventArgs e)
        {
            ViewState[CONST_SORTEXPRESSION_VIEWSTATE] = e.SortExpression;
            BindPeopleGrid();
        }

        protected void gvPeople_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvPeople.EditIndex = e.NewEditIndex;
            BindPeopleGrid();
        }

        protected void gvPeople_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvPeople.EditIndex = -1;
            BindPeopleGrid();
        }

        protected void gvPeople_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                int personID = Convert.ToInt32(gvPeople.DataKeys[e.RowIndex].Value);
                GridViewRow row = gvPeople.Rows[e.RowIndex];

                // Get the updated values from the edit row
                string personName = ((TextBox)row.Cells[2].Controls[0]).Text;
                string abbreviation = ((TextBox)row.Cells[3].Controls[0]).Text;
                bool enabled = ((CheckBox)row.Cells[4].Controls[0]).Checked;

                var repo = new PersonsRepository();
                var person = repo.GetById(personID);
                if (person != null)
                {
                    person.PersonName = personName;
                    person.Abbreviation = abbreviation;
                    person.Enabled = enabled;
                    repo.Update(person);
                }

                gvPeople.EditIndex = -1;
                BindPeopleGrid();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "TestPeople RowUpdating error: " + ex.Message);
            }
        }
    }
}
