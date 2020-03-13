using System;
using JPB.DataInputWindow.ViewModel;
using JPB.ErrorValidation;
using JPB.ErrorValidation.ValidationRules;
using JPB.ErrorValidation.ValidationTyps;

namespace JPB.DataInputWindow
{
	public class DataFieldBuilderFassade : DataInputBuilderFassade
	{
		private readonly DataInputBuilderFassade _parent;
		private readonly DataImportFieldViewModelBase _field;
		private readonly IErrorCollectionBase _errors;
		
		public DataFieldBuilderFassade(DataInputBuilderFassade parent, DisplayTypes typeKey, string key) : base(parent._dataImportViewModel)
		{
			_parent = parent;
			_field = InputFieldFactory(typeKey, out _errors);
			_field.Key = key;
			parent._dataImportViewModel.Fields.Add(_field);
		}

		public DataFieldBuilderFassade IsRequired(object errorText)
		{
			_errors.Add(new Error<DataImportFieldViewModelBase>(errorText, nameof(DataImportFieldViewModelBase.Value),
				f => f.Value == null));
			return this;
		}

		public DataFieldBuilderFassade Validate(object errorText, Func<object, bool> condition)
		{
			_errors.Add(new Error<DataImportFieldViewModelBase>(errorText, nameof(DataImportFieldViewModelBase.Value),
				f => condition(f.Value)));
			return this;
		}

		public DataFieldBuilderFassade Display(object caption)
		{
			_field.Caption = caption;
			return this;
		}

		private static DataImportFieldViewModelBase InputFieldFactory(DisplayTypes typeKey, out IErrorCollectionBase errors)
		{
			var type = DataImportWindowFactory.DisplayTypeLookup[typeKey];
			errors = new ThreadSaveValidationRuleBase(type);
			return Activator.CreateInstance(type, new object[]
			{
				errors
			}) as DataImportFieldViewModelBase;
		}
	}
}