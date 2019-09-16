using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using JPB.DynamicInputBox.InfoWindow;
using JPB.DynamicInputBox.InfoWindow.Controls;
using JPB.DynamicInputBox.InfoWindow.Wrapper;
using System.Collections.ObjectModel;

namespace JPB.DynamicInputBox
{
    public class Localisation
    {
        public Localisation()
        {
            Resources = new ConcurrentDictionary<string, string>();
        }

        public IDictionary<string, string> Resources { get; set; }
    }

    public static class InputWindow
    {
        static InputWindow()
        {
            Localisations = new ConcurrentDictionary<string, Localisation>();
        }

        private static void CreateDe()
        {
            var deLoc = new Localisation();
            deLoc.Resources.Add("EMPTY", "Leer");
        }

        public static Localisation CurrentLocalisation { get; set; }
        public static IDictionary<string, Localisation> Localisations { get; set; }

        public static T ReparseList<T>(IEnumerable<T> input, IListBoxItemWrapper selected) where T : class
        {
            return input.ElementAt(selected.Index);
        }

        public static T ReparseList<T>(IEnumerable<T> input, IEnumerable<IListBoxItemWrapper> selected) where T : class
        {
            return input.ElementAt(selected.FirstOrDefault().Index);
        }

        public static IEnumerable<string> ParseMultiItemsList(object elementAt)
        {
            return ParseMultiItemsList(elementAt as IEnumerable<IListBoxItemWrapper>);
        }

        public static IEnumerable<string> ParseMultiItemsList(IEnumerable<IListBoxItemWrapper> input)
        {
            return input.Select(s => s.Text);
        }

        public static T ReparseList<T>(IEnumerable<T> input, object selected)
        {
            if (selected is IListBoxItemWrapper)
            {
	            return input.ElementAt((selected as IListBoxItemWrapper).Index);
            }

            if (selected is IEnumerable<IListBoxItemWrapper>)
            {
	            return input.ElementAt((selected as IEnumerable<IListBoxItemWrapper>).First().Index);
            }

            return default(T);
        }

        public static object ShowInput<T>(IWaiterWrapper<T> inputQuestion)
        {
            var returns = new ObservableCollection<object>();
            if (WindowThread(new List<object> { inputQuestion }, () => returns,
                new List<InputMode> { InputMode.ShowProgress }))
            {
	            return returns.FirstOrDefault();
            }

            return null;
        }

        public static object ShowInput(Func<object> inputQuestion)
        {
            var returns = new ObservableCollection<object>();
            if (WindowThread(new List<object> { inputQuestion }, () => returns,
                new List<InputMode> { InputMode.ShowProgress }))
            {
	            return returns.FirstOrDefault();
            }

            return null;
        }

        public static string ShowInput(string inputQuestion)
        {
            var returns = new ObservableCollection<object>();
            if (WindowThread(new List<object> { inputQuestion }, () => returns, new List<InputMode> { InputMode.Text }))
            {
	            return returns.FirstOrDefault() as string;
            }

            return null;
        }

        #region InputMode Helper

        public static string ShowSingelLineInput(string header)
        {
            return ShowInput(header, InputMode.Text) as string;
        }

        public static long? ShowNumberInput(string header)
        {
            return (long?)ShowInput(header, InputMode.Number);
        }

        public static string ShowRichTextInput(string header)
        {
            return ShowInput(header, InputMode.RichText) as string;
        }

        public static string ShowSingelSelectInput(string header, params string[] values)
        {
            var wrapper = ShowInput(values.Aggregate(header, (e, f) => "#q" + e + f), InputMode.RadioBox) as IListBoxItemWrapper;
            return values[wrapper.Index];
        }

        public static string[] ShowMultiSelectInput(string header, params string[] values)
        {
            var wrapper = ShowInput(values.Aggregate(header, (e, f) => "#q" + e + f), InputMode.CheckBox) as IListBoxItemWrapper[];
            return wrapper.Select(s => s.Text).ToArray();
        }

        public static bool[] ShowMultiBooleanInput(string header, params string[] values)
        {
            var wrapper = ShowInput(values.Aggregate(header, (e, f) => "#q" + e + f), InputMode.CheckBox) as IListBoxItemWrapper[];
            return wrapper.Select(s => s.IsChecked).ToArray();
        }

        public static T ShowActionInput<T>(string header, Func<T> values)
        {
            IWaiterWrapper<T> wrapper = new WaiterWrapperImpl<T>(values, header);
            var showInput = ShowInput<T>(wrapper);
            if (showInput is T)
            {
	            return (T)showInput;
            }

            return default(T);
        }

        #endregion

        public static object ShowInput(string inputQuestion, InputMode modus)
        {
            var returns = new ObservableCollection<object>();
            if (WindowThread(new List<object> { inputQuestion }, () => returns, new List<InputMode> { modus }))
            {
	            return returns.FirstOrDefault();
            }

            return null;
        }

        public static bool ShowInput(Func<ObservableCollection<object>> updateDelegate, List<object> inputQuestions,
            List<InputMode> eingabeModi)
        {
            return WindowThread(inputQuestions, updateDelegate, eingabeModi);
        }

        public static IEnumerable<object> ShowInput(List<object> inputQuestions, List<InputMode> eingabeModi)
        {
            var returns = new ObservableCollection<object>();
            WindowThread(inputQuestions, () => returns, eingabeModi);
            return returns;
        }

        public static IEnumerable<object> ShowInput(List<object> inputQuestions)
        {
            var returns = new ObservableCollection<object>();
            WindowThread(inputQuestions, () => returns, returns.Select(retursn => InputMode.Text));
            return returns;
        }

        private static bool WindowThread(List<object> inputQuestions, Func<ObservableCollection<object>> returnlist,
            IEnumerable<InputMode> eingabeModi)
        {
            bool? ret = false;
            Thread windowThread = null;
            windowThread = new Thread(() =>
            {
                var fromThread = Dispatcher.CurrentDispatcher;
                var inputwindow = new UserInputWindow(inputQuestions, returnlist, eingabeModi, fromThread);
                ret = inputwindow.ShowDialog();
                fromThread.InvokeShutdown();
            });
            windowThread.Name = "InputWindowThread";
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.Start();
            windowThread.Join();
            return ret.HasValue && ret.Value;
        }

        public static string ParseListWithQustion(string pleaseSelectTheServer, string[] toArray)
        {
            return toArray.Aggregate(pleaseSelectTheServer, (current, s) => current + ("#q" + s));
        }
    }
}