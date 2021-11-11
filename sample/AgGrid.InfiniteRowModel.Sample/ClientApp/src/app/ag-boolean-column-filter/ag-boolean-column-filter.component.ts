import { AgFilterComponent } from 'ag-grid-angular';
import { IDoesFilterPassParams, IFilterParams } from 'ag-grid-community';
import { Component } from '@angular/core';
import { BooleanFilterModel } from './boolean-filter.model';
import { MAT_CHECKBOX_CLICK_ACTION } from '@angular/material';

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
