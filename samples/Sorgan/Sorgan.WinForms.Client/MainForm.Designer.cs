using System.Windows.Forms;

namespace Sorgan.WinForms.Client;

partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        LoginButton = new Button();
        SuspendLayout();
        // 
        // LoginButton
        // 
        LoginButton.Location = new System.Drawing.Point(243, 191);
        LoginButton.Margin = new Padding(4, 3, 4, 3);
        LoginButton.Name = "LoginButton";
        LoginButton.Size = new System.Drawing.Size(456, 96);
        LoginButton.TabIndex = 1;
        LoginButton.Text = "Log in using GitHub";
        LoginButton.UseVisualStyleBackColor = true;
        LoginButton.Click += LoginButton_Click;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(933, 519);
        Controls.Add(LoginButton);
        Margin = new Padding(4, 3, 4, 3);
        Name = "MainForm";
        Text = "OpenIddict Sorgan WinForms client";
        ResumeLayout(false);
    }

    #endregion

    private Button LoginButton;
}