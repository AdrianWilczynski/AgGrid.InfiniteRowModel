import { Component } from '@angular/core';
import { GridOptions, IFilterOptionDef, IGetRowsParams } from 'ag-grid-community';
import { AgBooleanColumnFilterComponent } from './ag-boolean-column-filter/ag-boolean-column-filter.component';
import { UserService } from './user.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent {
  constructor(private userService: UserService) { }

  nullFilterOption: IFilterOptionDef = {
    displayKey: 'null',
    displayName: 'Null',
    test: (filterValue, cellValue) => cellValue === null,
    hideFilterInput: true
  };

  notNullFilterOption: IFilterOptionDef = {
    displayKey: 'notNull',
    displayName: 'Not null',
    test: (filterValue, cellValue) => cellValue !== null,
    hideFilterInput: true
  };

  gridOptions: GridOptions = {
    defaultColDef: {
      sortable: true,
      floatingFilter: true
    },
    frameworkComponents: {
      'agBooleanColumnFilter': AgBooleanColumnFilterComponent
    },
    columnDefs: [
      {
        headerName: 'Full name',
        field: 'fullName',
        filter: 'agTextColumnFilter',
        filterParams: {
          filterOptions: [
            'equals', 'notEqual', 'contains', 'notContains', 'startsWith', 'endsWith',
            this.nullFilterOption, this.notNullFilterOption
          ]
        }
      },
      { headerName: 'Registered on', field: 'registeredOn', filter: 'agDateColumnFilter', sort: 'desc' },
      { headerName: 'Age', field: 'age', filter: 'agNumberColumnFilter' },
      { headerName: 'Is verified', field: 'isVerified', filter: 'agBooleanColumnFilter' }
    ],
    rowModelType: 'infinite',
    datasource: {
      getRows: (params: IGetRowsParams) => {
        console.log(JSON.stringify(params, null, 4));

        this.userService.getUsers(JSON.stringify(params))
          .subscribe(
            result => params.successCallback(result.rowsThisBlock, result.lastRow),
            () => params.failCallback());
      }
    }
  };
}
