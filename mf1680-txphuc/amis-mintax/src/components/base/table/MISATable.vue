<template>
  <div class="ms-table">
    <DxDataGrid
      id="grid-container"
      :data-source="dataSource"
      :key-expr="keyExpr"
      :allow-column-resizing="true"
      column-resizing-mode="widget"
      :allow-column-reordering="true"
      :column-auto-width="true"
      :show-row-lines="true"
      :show-column-lines="false"
      :show-borders="false"
      :hover-state-enabled="true"
      @selection-changed="$emit('selection-changed', $event)"
      @row-dbl-click="$emit('row-dbl-click', $event)"
      @cell-click="onSortChange"
      ref="dataGridRef"
    >
      <!-- Cột chọn bản ghi -->
      <DxColumn type="selection" :width="44" :allow-resizing="false" />
      <DxSelection mode="multiple" show-check-boxes-mode="always" />

      <!-- Các cột dữ liệu -->
      <DxColumn
        v-for="(column, index) in columns"
        :key="index"
        v-bind="column"
        :visible="column.visible === true || column.visible === undefined ? true : false"
        :allow-sorting="!apiSort"
        header-cell-template="headerCellTemplate"
        cell-template="customCell"
      />

      <!-- Tuỳ chỉnh cột bất kỳ -->
      <template #customCell="{ data }">
        <slot :name="data?.column?.dataField" v-bind="data">
          {{ data.text }}
        </slot>
      </template>

      <!-- Tuỳ chỉnh header -->
      <template #headerCellTemplate="{ data }">
        <div class="ms-table__header-cell">
          {{ data.column.caption }}

          <span
            v-if="apiSort && data.column.dataField === sort.sortColumn"
            :class="['ms-table__sort-icon', `--order-${sort.sortOrder}`]"
          >
            <MISAIcon size="20" icon="arrow-down" />
          </span>
          <div
            @click.stop="onFixedColumChange(data.columnIndex)"
            :class="['ms-table__pin-icon', { '--active': data.columnIndex === fixedColumnIndex }]"
          >
            <MISAIcon size="20" icon="pin" />
          </div>
        </div>
      </template>

      <!-- Cột action -->
      <DxColumn
        v-if="actionColumnEnabled"
        cell-template="cellTemplate"
        css-class="ms-table__action-column"
      ></DxColumn>
      <template #cellTemplate="{ data }">
        <div class="ms-table__action-container">
          <MISAButton type="rounded" color="secondary">
            <template slot="icon">
              <MISAIcon size="20" icon="new-window" />
            </template>
          </MISAButton>

          <MISAButton @click="$emit('edit-row', data)" type="rounded" color="secondary">
            <template slot="icon">
              <MISAIcon size="20" icon="pen" />
            </template>
          </MISAButton>

          <MISAButton @click="$emit('delete-row', data)" type="rounded" color="secondary">
            <template slot="icon">
              <MISAIcon color="#eb3333" size="20" icon="trash" />
            </template>
          </MISAButton>
        </div>
      </template>

      <!-- Ẩn phân trang mặc định -->
      <DxPager :allowed-page-sizes="[1000]" />
      <DxPaging :page-size="1000" />
    </DxDataGrid>
  </div>
</template>

<script>
import { DxDataGrid, DxColumn, DxSelection, DxPager, DxPaging } from "devextreme-vue/data-grid";
import MISAButton from "../button/MISAButton.vue";
import MISAIcon from "../icon/MISAIcon.vue";
import enums from "@/enum/enum";

export default {
  name: "MISATable",
  components: {
    DxDataGrid,
    DxColumn,
    DxSelection,
    MISAButton,
    MISAIcon,
    DxPager,
    DxPaging,
  },
  emits: [
    "selection-changed",
    "row-dbl-click",
    "sort-change",
    "fixed-column-change",
    "edit-row",
    "delete-row",
  ],
  props: {
    // Các cột của bảng
    columns: {
      type: Array,
      default() {
        return [];
      },
    },

    // Dữ liệu của bảng
    dataSource: {
      type: Array,
      default() {
        return [];
      },
    },

    // Trường lấy làm khoá
    keyExpr: {
      type: String,
      default: "",
    },

    // Sắp xếp dữ liệu trong bảng
    // Gồm các thuộc tính:
    // - sortColumn: String (Cột cần sắp xếp)
    // - sortOrder: String (Loại sắp xếp: asc | desc)
    sort: {
      type: Object,
    },

    apiSort: {
      type: Boolean,
      default: false,
    },

    // Hiện/ẩn cột action
    actionColumnEnabled: {
      type: Boolean,
      default: true,
    },
  },
  data: function () {
    return {};
  },
  computed: {
    /**
     * Description: Lấy vị trí cột được ghim
     * Author: txphuc (17/08/2023)
     */
    fixedColumnIndex() {
      // Tìm những cột được ghim và không bị ẩn
      const fixedColumns = this.columns.filter(
        (column) => column.fixed === true && column.visible !== false
      );

      let pinIndex = fixedColumns.length || null;

      return pinIndex;
    },
  },
  methods: {
    /**
     * Description: Xử lý chọn cột sắp xếp dữ liệu
     * Author: txphuc (17/08/2023)
     */
    onSortChange(sortColumn) {
      if (sortColumn.rowType === "header") {
        const dataField = sortColumn.column?.dataField;

        if (this.sort?.sortColumn === dataField && this.sort?.sortOrder === enums.sort.ASC) {
          // Chuyển từ sắp xếp TĂNG DẦN sang GIẢM DẦN
          this.$emit("sort-change", { dataField, sortOrder: enums.sort.DESC });
        } else if (
          this.sort?.sortColumn === dataField &&
          this.sort?.sortOrder === enums.sort.DESC
        ) {
          // Chuyển từ sắp xếp GIẢM DẦN sang KHÔNG SẮP XẾP
          this.$emit("sort-change", { sortColumn: null, sortOrder: null });
        } else {
          // Chuyển từ KHÔNG SẮP XẾP sang sắp xếp TĂNG DẦN
          this.$emit("sort-change", { dataField, sortOrder: enums.sort.ASC });
        }
      }
    },

    /**
     * Description: Bỏ chọn tất cả các dòng
     * Author: txphuc (17/08/2023)
     */
    clearAllSelection() {
      this.$refs["dataGridRef"].instance.clearSelection();
    },

    /**
     * Description: Xử lý ghim cột
     * Author: txphuc (17/08/2023)
     */
    onFixedColumChange(columnIndex) {
      // Loại bỏ tham chiếu tránh thay đổi mảng gốc
      // (làm cho component không được render lại)
      let localColumns = this.columns.map((col) => ({ ...col }));

      if (this.fixedColumnIndex === columnIndex) {
        // Bỏ ghim nếu bấm ghim hai lần vào một cột thì bỏ ghim
        localColumns = this.columns.map((column) => {
          column.fixed = false;

          return column;
        });
      } else {
        // Ghim cột hiện tại và cột trước nó
        localColumns = this.columns.map((column, index) => {
          column.fixed = false;

          if (index < columnIndex) {
            column.fixed = true;
          }

          return column;
        });
      }

      this.$emit("fixed-column-change", localColumns);
    },
  },
};
</script>

<style lang="scss">
@import url("./table.scss");
</style>