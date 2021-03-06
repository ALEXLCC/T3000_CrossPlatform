﻿namespace T3000.Controls
{
    using System;
    using System.Windows.Forms;
    using System.Collections.Generic;

    using EditAction =
        System.Action<object, System.EventArgs, object[]>;
    using ValidationFunc =
        System.Func<System.Windows.Forms.DataGridViewCell, object[], bool>;
    using CellAction =
        System.Action<object, System.Windows.Forms.DataGridViewCellEventArgs, object[]>;
    using FormatingFunc =
        System.Func<object, object>;

    public class TView : DataGridView
    {
        public bool RowIndexIsValid(int index) =>
            index >= 0 && index < RowCount;

        public TView() : base()
        {
            CellValueChanged += OnCellValueChanged;
        }

        #region User input handles

        public bool UserInputEnabled { get; set; } = true;

        private Dictionary<string, EventHandler> InputHandles = new Dictionary<string, EventHandler>();
        private Dictionary<string, EditAction> InputActions = new Dictionary<string, EditAction>();
        private Dictionary<string, object[]> InputArguments { get; set; } =
            new Dictionary<string, object[]>();

        private void InvokeInputHandle(string name, EventArgs e)
        {
            if (name == null || !UserInputEnabled)
            {
                return;
            }

            if (InputActions.ContainsKey(name))
            {
                var arguments = InputArguments.ContainsKey(name)
                    ? InputArguments[name] : new object[0];

                InputActions[name]?.Invoke(this, e, arguments);
            }
            if (InputHandles.ContainsKey(name))
            {
                InputHandles[name]?.Invoke(this, e);
            }
        }

        private bool InputHandleExists(string name) =>
            InputHandles.ContainsKey(name) || InputActions.ContainsKey(name);

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    var cell = CurrentCell;
                    if (cell == null || cell.ReadOnly)
                    {
                        break;
                    }

                    e.Handled = true;

                    var name = cell.ColumnName();
                    if (name != null && InputHandleExists(name))
                    {
                        InvokeInputHandle(name, e);
                        return;
                    }

                    BeginEdit(true);
                    return;
            }

            base.OnKeyDown(e);
        }

        protected override void OnCellContentClick(DataGridViewCellEventArgs e)
        {
            var cell = CurrentCell;
            if (cell != null)
            {
                var name = cell.ColumnName();
                if (!cell.ReadOnly && name != null && InputHandleExists(name))
                {
                    InvokeInputHandle(name, e);
                    return;
                }
            }

            base.OnCellContentClick(e);
        }

        public void AddEditHandler(string columnName, EventHandler handler)
        {
            InputHandles[columnName] = handler;
        }

        public void AddEditHandler(DataGridViewColumn column, EventHandler handler)
            => AddEditHandler(column.Name, handler);

        public void AddEditAction(string columnName, EditAction handler, params object[] arguments)
        {
            InputActions[columnName] = handler;
            InputArguments[columnName] = arguments;
        }

        public void AddEditAction(DataGridViewColumn column,
            EditAction handler, params object[] arguments) =>
            AddEditAction(column.Name, handler, arguments);

        public void ChangeEditAction(string columnName, EditAction handler) =>
            InputActions[columnName] = handler;

        public void ChangeEditAction(DataGridViewColumn column, EditAction handler) =>
            ChangeEditAction(column.Name, handler);

        public void ChangeEditArguments(string columnName, params object[] arguments) =>
            InputArguments[columnName] = arguments;

        public void ChangeEditArguments(DataGridViewColumn column, params object[] arguments) =>
            ChangeEditArguments(column.Name, arguments);

        #endregion

        #region Validation handles

        public bool ValidationEnabled { get; set; } = true;

        private Dictionary<string, ValidationFunc> ValidationHandles { get; set; } =
            new Dictionary<string, ValidationFunc>();
        private Dictionary<string, object[]> ValidationArguments { get; set; } =
            new Dictionary<string, object[]>();

        protected override void OnCellValidating(DataGridViewCellValidatingEventArgs e)
        {
            if (!RowIndexIsValid(e.RowIndex))
            {
                return;
            }

            try
            {
                var cell = Rows[e.RowIndex].Cells[e.ColumnIndex];
                ValidateCell(cell);
            }
            catch (Exception) { }

            base.OnCellValidating(e);
        }

        protected override void OnCellValueChanged(DataGridViewCellEventArgs e)
        {
            base.OnCellValueChanged(e);

            if (!RowIndexIsValid(e.RowIndex))
            {
                return;
            }

            var cell = Rows[e.RowIndex].Cells[e.ColumnIndex];
            ValidateCell(cell);
        }

        public bool ValidateCell(DataGridViewCell cell)
        {
            if (!ValidationEnabled)
            {
                return true;
            }

            if (cell == null)
            {
                return false;
            }

            var name = cell.ColumnName();
            if (name != null && ValidationHandles.ContainsKey(name))
            {
                var arguments = ValidationArguments.ContainsKey(name)
                    ? ValidationArguments[name] : new object[0];

                return ValidationHandles[name]?.Invoke(cell, arguments) ?? true;
            }

            return true;
        }

        public bool ValidateRow(DataGridViewRow row)
        {
            if (row == null)
            {
                return false;
            }

            var isValidated = true;
            foreach (DataGridViewCell cell in row.Cells)
            {
                isValidated &= ValidateCell(cell);
            }

            return isValidated;
        }

        public bool Validate()
        {
            var rows = Rows;
            if (rows == null)
            {
                return false;
            }

            var isValidated = true;
            foreach (DataGridViewRow row in rows)
            {
                isValidated &= ValidateRow(row);
            }

            return isValidated;
        }

        public void AddValidation(string columnName, ValidationFunc func, params object[] arguments)
        {
            ValidationHandles[columnName] = func;
            ValidationArguments[columnName] = arguments;
        }

        public void AddValidation(DataGridViewColumn column, ValidationFunc func, params object[] arguments) =>
            AddValidation(column.Name, func, arguments);

        public void ChangeValidationFunc(string columnName, ValidationFunc func) =>
            ValidationHandles[columnName] = func;

        public void ChangeValidationFunc(DataGridViewColumn column, ValidationFunc func) =>
            ChangeValidationFunc(column.Name, func);

        public void ChangeValidationArguments(string columnName, params object[] arguments) =>
            ValidationArguments[columnName] = arguments;

        public void ChangeValidationArguments(DataGridViewColumn column, params object[] arguments) =>
            ChangeValidationArguments(column.Name, arguments);

        #endregion

        #region Value changed handles

        public bool ValueChangedEnabled { get; set; } = true;

        private Dictionary<string, CellAction> ValueChangedHandles { get; set; } =
            new Dictionary<string, CellAction>();
        private Dictionary<string, object[]> ValueChangedArguments { get; set; } =
            new Dictionary<string, object[]>();

        private void OnCellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                var row = this.GetRow(e.RowIndex);
                if (row == null)
                {
                    return;
                }

                var cell = row.Cells[e.ColumnIndex];
                if (cell == null)
                {
                    return;
                }

                var name = cell.ColumnName();
                if (ValueChangedEnabled && 
                    name != null && 
                    ValueChangedHandles.ContainsKey(name))
                {
                    var arguments = ValueChangedArguments.ContainsKey(name)
                        ? ValueChangedArguments[name] : new object[0];

                    ValueChangedHandles[name]?.Invoke(this, e, arguments);
                }

                ValidateRow(row);
            }
            catch (Exception) { }
        }

        public void AddChangedHandler(string columnName, CellAction handler, params object[] arguments)
        {
            ValueChangedHandles[columnName] = handler;
            ValueChangedArguments[columnName] = arguments;
        }

        public void AddChangedHandler(DataGridViewColumn column, CellAction handler, params object[] arguments) =>
            AddChangedHandler(column.Name, handler, arguments);

        public void ChangeChangedHandler(string columnName, CellAction handler) =>
            ValueChangedHandles[columnName] = handler;

        public void ChangeChangedHandler(DataGridViewColumn column, CellAction handler) =>
            ChangeChangedHandler(column.Name, handler);

        public void ChangeChangedArguments(string columnName, params object[] arguments) =>
            ValueChangedArguments[columnName] = arguments;

        public void ChangeChangedArguments(DataGridViewColumn column, params object[] arguments) =>
            ChangeChangedArguments(column.Name, arguments);

        public void SendChanged(DataGridViewColumn column)
        {
            foreach (DataGridViewRow row in column.DataGridView.Rows)
            {
                OnCellValueChanged(new DataGridViewCellEventArgs(column.Index, row.Index));
            }
        }

        #endregion

        #region Formatting handles

        public bool FormattingEnabled { get; set; } = true;

        private Dictionary<string, FormatingFunc> FormattingHandles { get; set; } =
            new Dictionary<string, FormatingFunc>();

        protected override void OnCellFormatting(DataGridViewCellFormattingEventArgs e)
        {
            if (!RowIndexIsValid(e.RowIndex))
            {
                return;
            }

            try
            {
                var row = this.GetRow(e.RowIndex);
                if (row == null)
                {
                    return;
                }

                var cell = row.Cells[e.ColumnIndex];
                if (cell == null)
                {
                    return;
                }

                var name = cell.ColumnName();
                if (FormattingEnabled &&
                    name != null && 
                    FormattingHandles.ContainsKey(name))
                {
                    e.Value = FormattingHandles[name].Invoke(cell.Value);
                    e.FormattingApplied = true;
                    return;
                }

                ValidateCell(cell);
            }
            catch (Exception) { }

            base.OnCellFormatting(e);
        }

        public void AddFormating(DataGridViewColumn column, FormatingFunc func)
        {
            FormattingHandles[column.Name] = func;
        }

        #endregion

    }
}
