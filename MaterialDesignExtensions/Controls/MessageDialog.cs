﻿using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

using MaterialDesignThemes.Wpf;

namespace MaterialDesignExtensions.Controls
{
    /// <summary>
    /// The base class for simple dialogs with a title and a message.
    /// </summary>
    public abstract class MessageDialog : Control
    {
        private static readonly string OkButtonName = "okButton";

        /// <summary>
        /// The message to display inside the dialog.
        /// </summary>
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            nameof(Message), typeof(string), typeof(MessageDialog));

        /// <summary>
        /// The message to display inside the dialog.
        /// </summary>
        public string Message
        {
            get
            {
                return (string)GetValue(MessageProperty);
            }

            set
            {
                SetValue(MessageProperty, value);
            }
        }

        /// <summary>
        /// The label of the OC button.
        /// </summary>
        public static readonly DependencyProperty OkButtonLabelProperty = DependencyProperty.Register(
            nameof(OkButtonLabel), typeof(string), typeof(MessageDialog));

        /// <summary>
        /// The label of the OC button.
        /// </summary>
        public string OkButtonLabel
        {
            get
            {
                return (string)GetValue(OkButtonLabelProperty);
            }

            set
            {
                SetValue(OkButtonLabelProperty, value);
            }
        }

        /// <summary>
        /// The title to display inside the dialog.
        /// </summary>
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title), typeof(string), typeof(MessageDialog));

        /// <summary>
        /// The title to display inside the dialog.
        /// </summary>
        public string Title
        {
            get
            {
                return (string)GetValue(TitleProperty);
            }

            set
            {
                SetValue(TitleProperty, value);
            }
        }

        private Button m_okButton;

        static MessageDialog()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MessageDialog), new FrameworkPropertyMetadata(typeof(MessageDialog)));
        }

        /// <summary>
        /// Creates a new <see cref="MessageDialog" />.
        /// </summary>
        public MessageDialog()
            : base()
        {
            m_okButton = null;

            Loaded += LoadedHandler;
            Unloaded += UnloadedHandler;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (m_okButton != null)
            {
                m_okButton.Click -= OkButtonClickHandler;
            }

            m_okButton = Template.FindName(OkButtonName, this) as Button;
        }

        protected virtual void LoadedHandler(object sender, RoutedEventArgs args)
        {
            m_okButton.Click += OkButtonClickHandler;
        }

        protected virtual void UnloadedHandler(object sender, RoutedEventArgs args)
        {
            m_okButton.Click -= OkButtonClickHandler;
        }

        protected virtual void OkButtonClickHandler(object sender, RoutedEventArgs args)
        {
            DialogHost.CloseDialogCommand.Execute(null, GetDialogHost());
        }

        protected DialogHost GetDialogHost()
        {
            DependencyObject element = VisualTreeHelper.GetParent(this);

            while (element != null && !(element is DialogHost))
            {
                element = VisualTreeHelper.GetParent(element);
            }

            return element as DialogHost;
        }
    }

    /// <summary>
    /// The base class for arguments of simple dialogs with a title and a message.
    /// </summary>
    public class MessageDialogArguments
    {
        /// <summary>
        /// The message to display inside the dialog.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The label of the OC button.
        /// </summary>
        public string OkButtonLabel { get; set; }

        /// <summary>
        /// The title to display inside the dialog.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Callback after openening the dialog.
        /// </summary>
        public DialogOpenedEventHandler OpenedHandler { get; set; }

        /// <summary>
        /// Callback after closing the dialog.
        /// </summary>
        public DialogClosingEventHandler ClosingHandler { get; set; }

        /// <summary>
        /// Creates a new <see cref="MessageDialogArguments" />.
        /// </summary>
        public MessageDialogArguments()
        {
            Message = null;
            Title = null;
            OpenedHandler = null;
            ClosingHandler = null;
        }
    }
}
