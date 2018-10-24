WPFCommon
=========


The WPF Common libary is a shared collection of MVVM related classes. The repository contains some commonly used pattern like the ViewModelBase but also some additions to them like the AsyncViewModelBase that provides a common interface for starting async operations inside the ViewModel. Another addtion is a default implimentation of the INotifyDataErrorInfo interface for general usage with a collection of Rules. The repository contains a ThreadSaveObservableCollection that can be accessed across multiple thread and will sync all actions into the Dispatcher.


