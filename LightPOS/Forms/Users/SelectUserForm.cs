﻿//
// Copyright (c) NickAc. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using NickAc.LightPOS.Backend.Data;
using NickAc.LightPOS.Backend.Objects;
using NickAc.LightPOS.Backend.Translation;
using NickAc.LightPOS.Backend.Utils;
using NickAc.LightPOS.Frontend.Controls;

namespace NickAc.LightPOS.Frontend.Forms.Users
{
    public partial class SelectUserForm : TemplateForm
    {
        #region Constructors

        public SelectUserForm()
        {
            InitializeComponent();
            panel1.TileSize = TileSize;
            WindowState = FormWindowState.Maximized;
            translationHelper1.Translate(this);
        }

        #endregion

        public override Size MaximumSize
        {
            get => Size.Empty;
            set => base.MaximumSize = value;
        }

        #region Properties

        public User SelectedUser { get; set; }

        #endregion

        #region Fields

        private const int ControlPadding = 8;
        private const int MaxTilesPerRow = 3;
        private const int TileSize = 150;
        private IList<User> users;

        #endregion

        #region Methods

        public static User ShowUserSelectionDialog(bool canSelectCurrent = true)
        {
            var form = new SelectUserForm();
            form.CanSelectCurrentUser(canSelectCurrent).ShowDialog();
            return form.SelectedUser;
        }

        public SelectUserForm CanSelectCurrentUser(bool value)
        {
            panel1.CanSelectCurrentUser = value;
            return this;
        }

        public void Recenter(Control c, bool horizontal = true, bool vertical = true)
        {
            if (c == null) return;
            if (horizontal)
                c.Left = (c.Parent.ClientSize.Width - c.Width) / 2;
            if (vertical)
                c.Top = (c.Parent.ClientSize.Height - c.Height) / 2;
        }

        protected override void OnLoad(EventArgs e)
        {
            label1.Show();
            foreach (Control control in panel1.Controls) control.Dispose();
            panel1.Controls.Clear();
            Recenter(label1);
            panel1.Hide();
            base.OnLoad(e);
            var th = new Thread(() =>
            {
                InitEverything();

                users = DataManager.GetUsers();
                panel1.SetupUsers(users);
            });
            th.Start();
            panel1.UserClick += Panel1_UserClick;
            panel1.UserTilesCreated += Panel1_UserTilesCreated;
        }

        private void InitEverything()
        {
            Program.InitializeDatabase();

            //Get the translated administrator account username
            var adminUserName = "";
            using (var helper = new TranslationHelper())
            {
                adminUserName = helper.GetTranslation("create_user_admin");
            }

            var numberOfUsers = 0;
            numberOfUsers = DataManager.GetNumberOfUsers();
            if (numberOfUsers < 1)
            {
                this.InvokeIfRequired(Hide);
                //Create an administrator account
                Application.Run(new ModifyUserForm().WithName(adminUserName).WithPermissions(UserPermission.All));
                this.InvokeIfRequired(Show);
            }

            //The person might've not created a user
            //Check if it was created
            if (DataManager.GetNumberOfUsers() < 1)
            {
                //A new user wasn't created, so we'll exit the app.
                this.InvokeIfRequired(Close);
            }
        }

        private void Panel1_UserClick(object sender, UserPanel.UserEventArgs e)
        {
            //A user was selected. We can now close the form
            var usr = e.User;
            SelectedUser = usr;
            Close();
        }

        private void Panel1_UserTilesCreated(object sender, EventArgs e)
        {
            panel1.InvokeIfRequired(() =>
            {
                panel1.Show();
                Recenter(panel1);
            });
            label1.InvokeIfRequired(label1.Hide);
        }

        #endregion
    }
}