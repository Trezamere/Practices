using System;
using System.Diagnostics.Contracts;
using System.Windows.Forms;

namespace Practices.Mvvm.Services.FrameworkDialogs.OpenFile
{
	/// <summary>
	/// Class wrapping System.Windows.Forms.OpenFileDialog, making it accept a IOpenFileDialog.
	/// </summary>
	public class OpenFileDialog : IDisposable
	{
		private readonly IOpenFileDialog _openFileDialog;
		private System.Windows.Forms.OpenFileDialog _concreteOpenFileDialog;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="OpenFileDialog"/> class.
		/// </summary>
		/// <param name="openFileDialog">The interface of a open file dialog.</param>
		public OpenFileDialog(IOpenFileDialog openFileDialog)
		{
			Contract.Requires(openFileDialog != null);

			_openFileDialog = openFileDialog;

			// Create concrete OpenFileDialog
			_concreteOpenFileDialog = new System.Windows.Forms.OpenFileDialog
			{
				AddExtension = openFileDialog.AddExtension,
				CheckFileExists = openFileDialog.CheckFileExists,
				CheckPathExists = openFileDialog.CheckPathExists,
				DefaultExt = openFileDialog.DefaultExt,
				FileName = openFileDialog.FileName,
				Filter = openFileDialog.Filter,
				InitialDirectory = openFileDialog.InitialDirectory,
				Multiselect = openFileDialog.MultiSelect,
				Title = openFileDialog.Title
			};
		}
        
		/// <summary>
		/// Runs a common dialog box with the specified owner.
		/// </summary>
		/// <param name="owner">
		/// Any object that implements System.Windows.Forms.IWin32Window that represents the top-level
		/// window that will own the modal dialog box.
		/// </param>
		/// <returns>
		/// System.Windows.Forms.DialogResult.OK if the user clicks OK in the dialog box; otherwise,
		/// System.Windows.Forms.DialogResult.Cancel.
		/// </returns>
		public DialogResult ShowDialog(IWin32Window owner)
		{
			Contract.Requires(owner != null);

			DialogResult result = _concreteOpenFileDialog.ShowDialog(owner);

			// Update ViewModel
			_openFileDialog.FileName = _concreteOpenFileDialog.FileName;
			_openFileDialog.FileNames = _concreteOpenFileDialog.FileNames;

			return result;
		}
        
		#region IDisposable Members

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting
		/// unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~OpenFileDialog()
		{
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_concreteOpenFileDialog != null)
				{
					_concreteOpenFileDialog.Dispose();
					_concreteOpenFileDialog = null;
				}
			}
		}

		#endregion
	}
}
