using OptionsSample.Options;
using System.Collections.Generic;
using System.ComponentModel;

internal class GeneralOptions : BaseOptionModel<GeneralOptions>
{
    //[Category("My ")]
    [DisplayName("Characters")]
    [Description("Specifies the comma seperated characters to highlight")]
    [DefaultValue("\\u00A0")]
    public string Characters { get; set; } = "\\u00A0";
    // AssertEquals("this is some text", "this is some text");

}