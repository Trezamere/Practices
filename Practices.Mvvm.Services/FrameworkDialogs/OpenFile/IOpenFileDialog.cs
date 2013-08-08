﻿namespace Practices.Mvvm.Services.FrameworkDialogs.OpenFile
{
	/// <summary>
	/// Interface describing the OpenFileDialog.
	/// </summary>
	public interface IOpenFileDialog : IFileDialog
	{
		/// <summary>
		/// Gets or sets a value indicating whether the dialog box allows multiple files to be
		/// selected.
		/// </summary>
		bool MultiSelect { get; set; }
	}
}
