# AG Grid Infinite Row Model for ASP.NET Core & EF Core

AG Grid's [Infinite Row Model](https://www.ag-grid.com/javascript-grid/infinite-scrolling/) (infinite scroll)
for Entity Framework Core implemented with `System.Linq.Dynamic`.

## How to use?

- Add `AgGrid.InfiniteRowModel` package to your ASP.NET Core project.
- Call `GetInfiniteRowModelBlock` extension method on chosen `DbSet`/`IQueryable` and pass `IGetRowsParams` (without `context`) from AG Grid's `datasource`. You can pass deserialized object (of type `GetRowsParams`) or JSON string.
- Pass the result (`rowsThisBlock` and `lastRow`) to `successCallback` of your `datasource` (see sample code below).

*More in `sample/AgGrid.InfiniteRowModel.Sample`*

```csharp
[HttpGet]
public InfiniteRowModelResult<User> Get(string query)
{
    return _dbContext.Users.GetInfiniteRowModelBlock(query);
}
```

```ts
nullFilterOption: IFilterOptionDef = {
  displayKey: 'null',
  displayName: 'Null',
  test: (filterValues, cellValue) => cellValue === null,
  numberOfInputs: 0
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
        filterOptions: ['equals', 'notEqual', 'contains', 'notContains', 'startsWith', 'endsWith', this.nullFilterOption ]
      }
    },
    { headerName: 'Registered on', field: 'registeredOn', filter: 'agDateColumnFilter' },
    { headerName: 'Age', field: 'age', filter: 'agNumberColumnFilter' },
    { headerName: 'Is verified', field: 'isVerified', filter: 'agBooleanColumnFilter' }
  ],
  rowModelType: 'infinite',
  datasource: {
    getRows: (params: IGetRowsParams) => {
      this.userService.getUsers(JSON.stringify(params))
        .subscribe(
          result => params.successCallback(result.rowsThisBlock, result.lastRow),
          () => params.failCallback());
    }
  }
};
```

```ts
getUsers(query: string): Observable<InfiniteRowModelResult<User>> {
  return this.httpClient.get<InfiniteRowModelResult<User>>(this.baseUrl + 'api/Users', {
    params: {
      query: query
    }
  });
}
```

## Supported filters

This package supports all three [built-in simple filters](https://www.ag-grid.com/angular-data-grid/filter-provided-simple/):
- Text Filter,
- Number Filter,
- Date Filter.

You can use [custom filter option](https://www.ag-grid.com/angular-data-grid/filter-provided-simple/#custom-filter-options) with simple filters to allow filtering by null.

It also provides support for [custom boolean filter](https://www.ag-grid.com/angular-data-grid/component-filter/) implementation. You don't have to use the same filter but your filter model has to match `BooleanFilterModel` interface (below).

```ts
@Component({
  selector: 'app-ag-boolean-column-filter',
  templateUrl: './ag-boolean-column-filter.component.html',
  providers: [
    { provide: MAT_CHECKBOX_CLICK_ACTION, useValue: 'noop' }
  ]
})
export class AgBooleanColumnFilterComponent implements AgFilterComponent {
  private params: IFilterParams;

  value: boolean | null = null;

  agInit(params: IFilterParams): void {
    this.params = params;
  }

  onCheckboxClicked(): void {
    if (this.value === null) {
      this.value = true;
    } else if (this.value === true) {
      this.value = false;
    } else if (this.value === false) {
      this.value = null;
    }

    this.updateFilter();
  }

  isFilterActive(): boolean {
    return this.value !== null;
  }

  doesFilterPass(params: IDoesFilterPassParams): boolean {
    throw new Error(`Not implemented. Seems to be unnecessary since we're doing all our filtering on the server side.`);
  }

  getModel(): BooleanFilterModel {
    if (!this.isFilterActive()) {
      return null;
    }

    return {
      filter: this.value,
      filterType: 'boolean',
      type: 'equals'
    };
  }

  setModel(model: BooleanFilterModel) {
    if (!model) {
      this.value = null;
      return;
    }

    this.value = model.filter;
  }

  updateFilter() {
    this.params.filterChangedCallback();
  }

  getModelAsString(): string {
    return this.value.toString();
  }
}
```
```html
<div style="padding: 20px 20px 10px 20px;">
    <mat-checkbox color="primary" [checked]="value" [indeterminate]="value === null" (click)="onCheckboxClicked()">
    </mat-checkbox>
</div>
```

```ts
export interface BooleanFilterModel {
  filter: boolean;
  filterType: 'boolean';
  type: 'equals' | 'notEqual';
}
```

## How to run sample app?

- Just run it. Database will be created and seeded on app startup.
