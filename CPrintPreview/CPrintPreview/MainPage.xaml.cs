using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Printing;
using Windows.Graphics.Printing.OptionDetails;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Printing;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CPrintPreview
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        PrintManager printmgr = PrintManager.GetForCurrentView();        
        PrintDocument printDoc = null;          
        PrintTask task = null;
        DisplayContent imageText = DisplayContent.TextAndImages;
        public MainPage()
        {
            this.InitializeComponent();          
            printmgr.PrintTaskRequested += Printmgr_PrintTaskRequested;
        }
        private void Printmgr_PrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs args)
        {
            var deferral = args.Request.GetDeferral();   
                 
            task = args.Request.CreatePrintTask("Print", OnPrintTaskSourceRequrested);
            task.Completed += PrintTask_Completed;
            PrintTaskOptionDetails printDetailedOptions = PrintTaskOptionDetails.GetFromPrintTaskOptions(task.Options);
            IList<string> displayedOptions = printDetailedOptions.DisplayedOptions;   
            // Create a new list option
            PrintCustomItemListOptionDetails pageFormat = printDetailedOptions.CreateItemListOption("PageContent", "Pictures");
            pageFormat.AddItem("PicturesText", "Pictures and text");
            pageFormat.AddItem("PicturesOnly", "Pictures only");
            pageFormat.AddItem("TextOnly", "Text only");
            // Add the custom option to the option list
            displayedOptions.Add("PageContent");

            printDetailedOptions.OptionChanged += printDetailedOptions_OptionChanged;


            deferral.Complete();
        }

        private async void printDetailedOptions_OptionChanged(PrintTaskOptionDetails sender, PrintTaskOptionChangedEventArgs args)
        {
            string optionId = args.OptionId as string;
            if (string.IsNullOrEmpty(optionId))
            {
                return;
            }   

            if (optionId == "PageContent")
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    printDoc.InvalidatePreview();
                });
            }
        }

        private void PrintTask_Completed(PrintTask sender, PrintTaskCompletedEventArgs args)
        {
            
        }
        private async void OnPrintTaskSourceRequrested(PrintTaskSourceRequestedArgs args)
        {

            var def = args.GetDeferral();
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
              () =>
              {              
                  args.SetSource(printDoc?.DocumentSource);
              });
            def.Complete();
        }
        private async void appbar_Printer_Click(object sender, RoutedEventArgs e)
        {
            if (printDoc != null)
            {
                printDoc.GetPreviewPage -= OnGetPreviewPage;
                printDoc.Paginate -= PrintDic_Paginate;
                printDoc.AddPages -= PrintDic_AddPages;
            }
            this.printDoc = new PrintDocument();
            printDoc.GetPreviewPage += OnGetPreviewPage;  
            printDoc.Paginate += PrintDic_Paginate;     
            printDoc.AddPages += PrintDic_AddPages;       
            bool showPrint = await PrintManager.ShowPrintUIAsync();
        }

        private void PrintDic_AddPages(object sender, AddPagesEventArgs e)
        {    
            printDoc.AddPage(this);
            printDoc.AddPagesComplete();
        }

        private void PrintDic_Paginate(object sender, PaginateEventArgs e)
        {
            PrintTaskOptions opt = task.Options;        
            PrintTaskOptionDetails printDetailedOptions = PrintTaskOptionDetails.GetFromPrintTaskOptions(e.PrintTaskOptions);
            string pageContent = (printDetailedOptions.Options["PageContent"].Value as string).ToLowerInvariant();
            // Set the text & image display flag
            imageText = (DisplayContent)((Convert.ToInt32(pageContent.Contains("pictures")) << 1) | Convert.ToInt32(pageContent.Contains("text")));
            printDoc.SetPreviewPageCount(1, PreviewPageCountType.Final);
        }
        private void OnGetPreviewPage(object sender, GetPreviewPageEventArgs e)
        {           
            // printDoc.SetPreviewPage(e.PageNumber, sp_PrintArea);
            switch (imageText)
            {
                case DisplayContent.Images:
                    printDoc.SetPreviewPage(e.PageNumber, imgtest); break;
                case DisplayContent.Text:
                    printDoc.SetPreviewPage(e.PageNumber, Txtblock); break;
                case DisplayContent.TextAndImages:
                    printDoc.SetPreviewPage(e.PageNumber, this); break;
            }
        }         
    }
   
    [Flags]
    internal enum DisplayContent : int
    {
        /// <summary>
        /// Show only text
        /// </summary>
        Text = 1,

        /// <summary>
        /// Show only images
        /// </summary>
        Images = 2,

        /// <summary>
        /// Show a combination of images and text
        /// </summary>
        TextAndImages = 3
    }
}
