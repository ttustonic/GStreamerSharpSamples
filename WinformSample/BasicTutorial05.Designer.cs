namespace WinformSample
{
    partial class BasicTutorial5
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
            this.panel2 = new System.Windows.Forms.Panel();
            this.slider = new System.Windows.Forms.TrackBar();
            this.stopButton = new System.Windows.Forms.Button();
            this.pauseButton = new System.Windows.Forms.Button();
            this.playButton = new System.Windows.Forms.Button();
            this.videoPanel = new System.Windows.Forms.Panel();
            this.streamsList = new System.Windows.Forms.TextBox();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.slider)).BeginInit();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.slider);
            this.panel2.Controls.Add(this.stopButton);
            this.panel2.Controls.Add(this.pauseButton);
            this.panel2.Controls.Add(this.playButton);
            this.panel2.Location = new System.Drawing.Point(9, 399);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(633, 39);
            this.panel2.TabIndex = 2;
            // 
            // slider
            // 
            this.slider.Location = new System.Drawing.Point(297, 4);
            this.slider.Name = "slider";
            this.slider.Size = new System.Drawing.Size(333, 45);
            this.slider.TabIndex = 3;
            this.slider.ValueChanged += new System.EventHandler(this.OnSliderValueChanged);
            // 
            // stopButton
            // 
            this.stopButton.Location = new System.Drawing.Point(206, 4);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(75, 23);
            this.stopButton.TabIndex = 2;
            this.stopButton.Text = "STOP";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.OnStopClick);
            // 
            // pauseButton
            // 
            this.pauseButton.Location = new System.Drawing.Point(114, 3);
            this.pauseButton.Name = "pauseButton";
            this.pauseButton.Size = new System.Drawing.Size(75, 23);
            this.pauseButton.TabIndex = 1;
            this.pauseButton.Text = "PAUSE";
            this.pauseButton.UseVisualStyleBackColor = true;
            this.pauseButton.Click += new System.EventHandler(this.OnPauseClick);
            // 
            // playButton
            // 
            this.playButton.Location = new System.Drawing.Point(14, 3);
            this.playButton.Name = "playButton";
            this.playButton.Size = new System.Drawing.Size(75, 23);
            this.playButton.TabIndex = 0;
            this.playButton.Text = "PLAY";
            this.playButton.UseVisualStyleBackColor = true;
            this.playButton.Click += new System.EventHandler(this.OnPlayClick);
            // 
            // videoPanel
            // 
            this.videoPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.videoPanel.Location = new System.Drawing.Point(9, 12);
            this.videoPanel.Name = "videoPanel";
            this.videoPanel.Size = new System.Drawing.Size(633, 381);
            this.videoPanel.TabIndex = 3;
            // 
            // streamsList
            // 
            this.streamsList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.streamsList.Location = new System.Drawing.Point(649, 13);
            this.streamsList.Multiline = true;
            this.streamsList.Name = "streamsList";
            this.streamsList.Size = new System.Drawing.Size(147, 425);
            this.streamsList.TabIndex = 4;
            // 
            // BasicTutorial5
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.streamsList);
            this.Controls.Add(this.videoPanel);
            this.Controls.Add(this.panel2);
            this.Name = "BasicTutorial5";
            this.Text = "BasicTutorial5";
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.slider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TrackBar slider;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.Button pauseButton;
        private System.Windows.Forms.Button playButton;
        private System.Windows.Forms.Panel videoPanel;
        private System.Windows.Forms.TextBox streamsList;
    }
}

