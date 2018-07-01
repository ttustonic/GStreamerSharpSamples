using System;

namespace WinformSample
{
    partial class VideoOverlay
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;


        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.videoPanel = new System.Windows.Forms.Panel();
            this.playButton = new System.Windows.Forms.Button();
            this.pauseButton = new System.Windows.Forms.Button();
            this.stopButton = new System.Windows.Forms.Button();
            this.scale = new System.Windows.Forms.TrackBar();
            this.panel2 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this._lbl = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.scale)).BeginInit();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // videoPanel
            // 
            this.videoPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.videoPanel.Location = new System.Drawing.Point(7, 12);
            this.videoPanel.Name = "videoPanel";
            this.videoPanel.Size = new System.Drawing.Size(512, 352);
            this.videoPanel.TabIndex = 6;
            // 
            // playButton
            // 
            this.playButton.Location = new System.Drawing.Point(86, 105);
            this.playButton.Name = "playButton";
            this.playButton.Size = new System.Drawing.Size(75, 23);
            this.playButton.TabIndex = 0;
            this.playButton.Text = "PLAY";
            this.playButton.UseVisualStyleBackColor = true;
            this.playButton.Click += new System.EventHandler(this.OnPlayClick);
            // 
            // pauseButton
            // 
            this.pauseButton.Location = new System.Drawing.Point(167, 105);
            this.pauseButton.Name = "pauseButton";
            this.pauseButton.Size = new System.Drawing.Size(75, 23);
            this.pauseButton.TabIndex = 1;
            this.pauseButton.Text = "PAUSE";
            this.pauseButton.UseVisualStyleBackColor = true;
            this.pauseButton.Click += new System.EventHandler(this.OnPauseClick);
            // 
            // stopButton
            // 
            this.stopButton.Location = new System.Drawing.Point(5, 105);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(75, 23);
            this.stopButton.TabIndex = 2;
            this.stopButton.Text = "OPEN";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.OnOpenClick);
            // 
            // scale
            // 
            this.scale.Location = new System.Drawing.Point(5, 54);
            this.scale.Maximum = 100;
            this.scale.Name = "scale";
            this.scale.Size = new System.Drawing.Size(333, 45);
            this.scale.TabIndex = 3;
            this.scale.ValueChanged += new System.EventHandler(this.OnScaleValueChanged);
            this.scale.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnScaleMouseDown);
            this.scale.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnScaleMouseUp);
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.button1);
            this.panel2.Controls.Add(this._lbl);
            this.panel2.Controls.Add(this.scale);
            this.panel2.Controls.Add(this.stopButton);
            this.panel2.Controls.Add(this.pauseButton);
            this.panel2.Controls.Add(this.playButton);
            this.panel2.Location = new System.Drawing.Point(7, 369);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(512, 131);
            this.panel2.TabIndex = 5;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(249, 105);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 5;
            this.button1.Text = "STOP";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.OnStopClick);
            // 
            // _lbl
            // 
            this._lbl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._lbl.AutoSize = true;
            this._lbl.Location = new System.Drawing.Point(291, 105);
            this._lbl.Name = "_lbl";
            this._lbl.Size = new System.Drawing.Size(0, 13);
            this._lbl.TabIndex = 4;
            // 
            // VideoOverlay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(531, 513);
            this.Controls.Add(this.videoPanel);
            this.Controls.Add(this.panel2);
            this.Name = "VideoOverlay";
            this.Text = "VideoOverlay";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.scale)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel videoPanel;
        private System.Windows.Forms.Button playButton;
        private System.Windows.Forms.Button pauseButton;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.TrackBar scale;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label _lbl;
        private System.Windows.Forms.Button button1;
    }
}