using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Compilation;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Web.UI.WebControls.CompositeControl" />
    /// <seealso cref="Rock.Web.UI.Controls.IRockControl" />
    public class ItemFromBlockPicker : CompositeControl, IRockControl
    {
        #region IRockControl implementation

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text for the label." )
        ]
        public string Label
        {
            get { return ViewState["Label"] as string ?? string.Empty; }
            set { ViewState["Label"] = value; }
        }

        /// <summary>
        /// Gets or sets the form group class.
        /// </summary>
        /// <value>
        /// The form group class.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        Description( "The CSS class to add to the form-group div." )
        ]
        public string FormGroupCssClass
        {
            get { return ViewState["FormGroupCssClass"] as string ?? string.Empty; }
            set { ViewState["FormGroupCssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the CSS Icon text.
        /// </summary>
        /// <value>
        /// The CSS icon class.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text for the label." )
        ]
        public string IconCssClass
        {
            get { return ViewState["IconCssClass"] as string ?? string.Empty; }
            set { ViewState["IconCssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        /// <value>
        /// The help text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The help block." )
        ]
        public string Help
        {
            get
            {
                return HelpBlock != null ? HelpBlock.Text : string.Empty;
            }
            set
            {
                if ( HelpBlock != null )
                {
                    HelpBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the warning text.
        /// </summary>
        /// <value>
        /// The warning text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The warning block." )
        ]
        public string Warning
        {
            get
            {
                return WarningBlock != null ? WarningBlock.Text : string.Empty;
            }
            set
            {
                if ( WarningBlock != null )
                {
                    WarningBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RockTextBox"/> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "false" ),
        Description( "Is the value required?" )
        ]
        public bool Required
        {
            get { return ViewState["Required"] as bool? ?? false; }
            set { ViewState["Required"] = value; }
        }

        /// <summary>
        /// Gets or sets the required error message.  If blank, the LabelName name will be used
        /// </summary>
        /// <value>
        /// The required error message.
        /// </value>
        public string RequiredErrorMessage
        {
            get
            {
                return RequiredFieldValidator != null ? RequiredFieldValidator.ErrorMessage : string.Empty;
            }
            set
            {
                if ( RequiredFieldValidator != null )
                {
                    RequiredFieldValidator.ErrorMessage = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets an optional validation group to use.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get
            {
                return RequiredFieldValidator.ValidationGroup;
            }
            set
            {
                RequiredFieldValidator.ValidationGroup = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsValid
        {
            get
            {
                return !Required || RequiredFieldValidator == null || RequiredFieldValidator.IsValid;
            }
        }

        /// <summary>
        /// Gets or sets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        public HelpBlock HelpBlock { get; set; }

        /// <summary>
        /// Gets or sets the warning block.
        /// </summary>
        /// <value>
        /// The warning block.
        /// </value>
        public WarningBlock WarningBlock { get; set; }

        /// <summary>
        /// Gets or sets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemFromBlockPicker" /> class.
        /// </summary>
        public ItemFromBlockPicker()
            : base()
        {
            RequiredFieldValidator = new HiddenFieldValidator();
            HelpBlock = new HelpBlock();
            WarningBlock = new WarningBlock();
        }

        #endregion

        #region Fields

        private Panel _pickerPanel;
        private LinkButton _lbShowPicker;
        private ModalDialog _pickerDialog;
        private UserControl _pickerControl;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the button CSS class.
        /// </summary>
        /// <value>
        /// The button CSS class.
        /// </value>
        public string ButtonCssClass
        {
            get
            {
                EnsureChildControls();
                return _lbShowPicker.CssClass;
            }

            set
            {
                EnsureChildControls();
                _lbShowPicker.CssClass = value;
            }
        }

        /// <summary>
        /// Gets or sets the button text.
        /// </summary>
        /// <value>
        /// The button text.
        /// </value>
        public string ButtonText
        {
            get
            {
                EnsureChildControls();
                return _lbShowPicker.Text;
            }

            set
            {
                EnsureChildControls();
                _lbShowPicker.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the button text lava template.
        /// HINT: Use {{ SelectedText }} 
        /// </summary>
        /// <value>
        /// The button text template.
        /// </value>
        public string ButtonTextTemplate
        {
            get
            {
                return ViewState["ButtonTextTemplate"] as string;
            }

            set
            {
                ViewState["ButtonTextTemplate"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the BlockType.Guid to be used as the block that will present the Picker UI
        /// </summary>
        /// <value>
        /// The block type unique identifier.
        /// </value>
        public string BlockTypePath
        {
            get
            {
                return ViewState["BlockTypePath"] as string;
            }

            set
            {
                ViewState["BlockTypePath"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show in modal].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show in modal]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowInModal { get; set; }

        /// <summary>
        /// Shows the modal.
        /// </summary>
        public void ShowModal()
        {
            _pickerDialog.Show();
        }

        /// <summary>
        /// Gets the picker block.
        /// </summary>
        /// <value>
        /// The picker block.
        /// </value>
        public IPickerBlock PickerBlock
        {
            get
            {
                return _pickerControl as IPickerBlock;
            }
        }

        #endregion

        #region methods

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            _lbShowPicker = new LinkButton();
            _lbShowPicker.Click += _lbShowPicker_Click;
            this.Controls.Add( _lbShowPicker );

            _pickerDialog = new ModalDialog();
            _pickerDialog.SaveClick += _pickerDialog_SaveClick;

            this.Controls.Add( _pickerDialog );

            _pickerPanel = new Panel();
            //_pickerPanel.CssClass = $"picker picker-select picker-person {this.CssClass}";

            if ( BlockTypePath.IsNotNullOrWhiteSpace() )
            {
                _pickerControl = TemplateControl.LoadControl( BlockTypePath ) as UserControl;
                var rockPage = System.Web.HttpContext.Current.Handler as RockPage;

                var pageCache = PageCache.Get( rockPage.PageId );
                ( _pickerControl as RockBlock )?.SetBlock( pageCache, null, false, false );

                if ( ( _pickerControl as IPickerBlock ).ShowInModal )
                {
                    _pickerDialog.Content.Controls.Add( _pickerPanel );
                }
                else
                {
                    this.Controls.Add( _pickerPanel );
                }

                _pickerPanel.Controls.Add( _pickerControl );

                /*var pickerBlock = _rockBlock as IPickerBlock;
                if ( pickerBlock != null )
                {
                    pickerBlock.SelectItem += PickerBlock_SelectItem;
                }*/
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the _pickerDialog control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _pickerDialog_SaveClick( object sender, EventArgs e )
        {
            _pickerDialog.Hide();

            // if the picker was in a modal dialog, track the SelectValue and SelectedText in viewstate when saved 
            ViewState["SelectedValue"] = ( _pickerControl as IPickerBlock )?.SelectedValue;
            ViewState["SelectedText"] = ( _pickerControl as IPickerBlock )?.SelectedText;

            SelectItem?.Invoke( sender, e );
        }

        /// <summary>
        /// Handles the Click event of the _lbShowPicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _lbShowPicker_Click( object sender, EventArgs e )
        {
            ShowModal();
        }

        /// <summary>
        /// Handles the SelectItem event of the PickerBlock control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void PickerBlock_SelectItem( object sender, EventArgs e )
        {
            SelectItem?.Invoke( sender, e );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                RockControlHelper.RenderControl( this, writer );
            }
        }

        /// <summary>
        /// This is where you implement the simple aspects of rendering your control.  The rest
        /// will be handled by calling RenderControlHelper's RenderControl() method.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            base.RenderControl( writer );
        }

        /// <summary>
        /// Gets or sets the selected value.
        /// </summary>
        /// <value>
        /// The selected value.
        /// </value>
        public string SelectedValue
        {
            get
            {
                EnsureChildControls();
                var pickerBlock = _pickerControl as IPickerBlock;
                if ( pickerBlock?.ShowInModal == true )
                {
                    // if shown in a modal, track the SelectedValue in viewstate since the pickerBlock could be cancelled
                    return ViewState["SelectedValue"] as string;
                }
                else
                {
                    return pickerBlock?.SelectedValue;
                }
            }

            set
            {
                EnsureChildControls();
                var pickerBlock = _pickerControl as IPickerBlock;
                if ( pickerBlock != null )
                {
                    pickerBlock.SelectedValue = value;
                }
            }
        }

        /// <summary>
        /// Gets the selected text.
        /// </summary>
        /// <value>
        /// The selected text.
        /// </value>
        public string SelectedText
        {
            get
            {
                EnsureChildControls();
                var pickerBlock = _pickerControl as IPickerBlock;
                if ( pickerBlock?.ShowInModal == true )
                {
                    // if shown in a modal, track the SelectedText in viewstate since the pickerBlock could be cancelled
                    return ViewState["SelectedText"] as string;
                }
                else
                {
                    return pickerBlock?.SelectedText;
                }
            }
        }

        /// <summary>
        /// Occurs when [select item].
        /// </summary>
        public event EventHandler SelectItem;

        #endregion
    }
}
