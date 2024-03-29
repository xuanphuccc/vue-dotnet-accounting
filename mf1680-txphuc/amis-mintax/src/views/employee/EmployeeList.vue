<template>
  <div class="page-container">
    <div class="page__header">
      <div class="page__title-wrapper">
        <h1 class="page__title">{{ $t("employee.employeeList.title") }}</h1>
      </div>

      <div class="page__header-controls">
        <MISAButton color="secondary">
          {{ $t("employee.employeeList.button.syncAmisSystem") }}
          <template slot="icon">
            <MISAIcon icon="amis-system" />
          </template>
        </MISAButton>

        <MISAButton
          @click="
            () => {
              this.$router.push({ name: 'manage-license' });
            }
          "
          color="secondary"
        >
          {{ $t("employee.employeeList.button.manageEmployeeUsingService") }}
          <template slot="icon">
            <MISAIcon color="#007aff" icon="manage-license" no-color />
          </template>
        </MISAButton>
      </div>
    </div>

    <div class="page__content">
      <div class="controls-wrapper">
        <!-- Thông tin các hàng đang được chọn -->
        <div v-if="selectedRowsData?.length > 0" class="selection-container">
          <div class="selection-info">
            <span>{{ $t("text.selected") }}</span>
            <span class="text-bold selection-info__number">{{ selectedRowsData?.length }}</span>

            <div class="v-separate"></div>

            <MISAButton @click="clearAllSelection" type="link" color="danger">{{
              $t("button.unChecked")
            }}</MISAButton>
          </div>
          <div class="selection-controls">
            <div class="controls__group">
              <MISAButton color="secondary">
                {{ $t("employee.employeeList.button.createDeclaration") }}
                <template slot="icon">
                  <MISAIcon size="20" icon="plus" />
                </template>
              </MISAButton>

              <MISAButton color="secondary">
                {{ $t("employee.employeeList.button.createProcedure") }}
                <template slot="icon">
                  <MISAIcon size="20" icon="plus" />
                </template>
              </MISAButton>
            </div>

            <div class="controls__group">
              <MISAButton
                @click="exportListToExcel()"
                :loading="isLoadingExportExcel"
                color="secondary"
              >
                {{ $t("button.exportExcel") }}
                <template slot="icon">
                  <MISAIcon size="20" icon="export" />
                </template>
              </MISAButton>

              <MISAButton
                v-tooltip="'Ctrl + D'"
                @click="
                  showDeleteConfirmDialog(
                    $t('employee.dialog.deleteMultipleEmployeeDesc', {
                      quantity: selectedRowsData.length,
                    })
                  )
                "
                color="secondary"
              >
                {{ $t("button.delete") }}
                <template slot="icon">
                  <MISAIcon color="#eb3333" size="20" icon="trash" />
                </template>
              </MISAButton>
            </div>
          </div>
        </div>

        <!-- Filter, tìm kiếm, tuỳ chỉnh cột -->
        <div v-if="selectedRowsData?.length === 0" class="filter-container">
          <div class="filter__left">
            <div class="controls__group">
              <MISATextBox
                v-tooltip="$t('employee.filterBar.search')"
                @enter-key="applySearch"
                :placeholder="$t('employee.filterBar.search')"
              >
                <MISAIcon size="20" icon="search" />
              </MISATextBox>

              <MISATreeView
                v-model="filterRequest.department"
                :dataSource="departments"
                valueExpr="value"
                :placeholder="$t('employee.filterBar.department')"
                :searchPlaceholder="$t('placeholder.search')"
              />
            </div>

            <div class="controls__group">
              <MISAButton
                @click="downloadExcel()"
                :loading="isLoadingExportExcel"
                color="secondary"
              >
                {{ $t("button.exportExcel") }}
                <template slot="icon">
                  <MISAIcon :size="20" icon="export" />
                </template>
              </MISAButton>
            </div>
          </div>

          <div class="filter__right">
            <MISAButtonGroup>
              <MISAButton v-tooltip="'Ctrl + 1'" @click="openFormForCreate">
                {{ $t("button.addNew") }}
                <template slot="icon">
                  <MISAIcon size="20" icon="plus" />
                </template>
              </MISAButton>
              <MISAButton>
                <template slot="icon">
                  <MISAIcon size="16" icon="angle-down" />
                </template>
              </MISAButton>
            </MISAButtonGroup>

            <MISAButton
              v-tooltip="$t('employee.filterBar.filter')"
              @click="isOpenFilterPopup = true"
              :badge="detectFilter"
              color="secondary"
              id="employee-list-filter-button"
            >
              <template slot="icon">
                <MISAIcon size="20" icon="mintax-filter" />
              </template>
            </MISAButton>

            <MISAButton
              v-tooltip="$t('employee.filterBar.customizeColumns')"
              @click="isOpenTableCustomize = true"
              color="secondary"
            >
              <template slot="icon">
                <MISAIcon size="20" icon="setting-gear" />
              </template>
            </MISAButton>
          </div>
        </div>
      </div>

      <!-- Table -->
      <MISATable
        @selection-changed="onSelectionChanged"
        @row-dbl-click="onRowDoubleClick"
        @sort-change="onSortChange"
        @fixed-column-change="onFixedColumnChange"
        @new-window="onClickNewWindow"
        @edit-row="onClickEditRow"
        @delete-row="onClickDeleteRow"
        :sort="filterRequest"
        :columns="tableColumns"
        :dataSource="dataSource"
        keyExpr="EmployeeID"
        ref="tableRef"
      >
        <template #EmployeeStatus="data">
          <MISABadge :text="data.text" :color="data.value == 0 ? 'disabled' : 'success'" />
        </template>
      </MISATable>

      <!-- Phân trang -->
      <MISATableFooter
        @next-page="onNextPage"
        @prev-page="onPrevPage"
        @select-page-size="onPageSizeChange"
        :currentPage="filterRequest.page"
        :pageSize="filterRequest.pageSize"
        :totalRecords="pagingInfo.totalRecords"
        :totalPages="pagingInfo.totalPages"
      />

      <!-- Tuỳ chỉnh cột -->
      <MISATableCustomize
        @close="isOpenTableCustomize = false"
        @change="applyTableCustomize"
        v-if="isOpenTableCustomize"
        :columns="tableColumns"
        :defaultColumns="defaultColumns"
      />

      <!-- Bộ lọc nâng cao -->
      <MISAFilterPopup
        @close="isOpenFilterPopup = false"
        @apply="applyFilter"
        @reset="resetFilter"
        v-if="isOpenFilterPopup"
        :filterGroups="filterGroups"
        :defaultFilterGroups="defaultFilterGroups"
      />
    </div>
  </div>
</template>

<script>
import MISAIcon from "@/components/base/icon/MISAIcon.vue";
import MISAButton from "@/components/base/button/MISAButton.vue";
import MISAButtonGroup from "@/components/base/button/MISAButtonGroup.vue";
import MISATable from "@/components/base/table/MISATable.vue";
import MISATableFooter from "@/components/base/table-footer/MISATableFooter.vue";
import MISATableCustomize from "@/components/base/table-customize/MISATableCustomize.vue";
import MISATextBox from "@/components/base/text-box/MISATextBox.vue";
import MISATreeView from "@/components/base/tree-view/MISATreeView.vue";
import MISAFilterPopup from "@/components/base/filter-popup/MISAFilterPopup.vue";
import { employeeColumns, excelExportSheets } from "./employee-columns";
import employeeApi from "@/api/employee-api";
import { departments } from "@/api/mock-data";
import MISABadge from "@/components/base/badge/MISABadge.vue";
import { employeeFilterGroups } from "./employee-filter";
import { formatDate } from "devextreme/localization";
import { mapState } from "vuex";
import enums from "@/enum/enum";

const defaultFilter = {
  WorkStatus: null,
  WorkStatusFilterBy: null,
  EmployeeType: null,
  EmployeeTypeFilterBy: null,
  TaxCode: null,
  TaxCodeFilterBy: null,
  DateColumn: null,
  DateColumnFilterBy: null,
  DateStart: null,
  DateEnd: null,
};

export default {
  name: "EmployeeList",
  components: {
    MISAIcon,
    MISAButton,
    MISAButtonGroup,
    MISATable,
    MISATableFooter,
    MISATableCustomize,
    MISAFilterPopup,
    MISATextBox,
    MISATreeView,
    MISABadge,
  },
  data: function () {
    return {
      departments,

      dataSource: [],

      defaultColumns: [...employeeColumns],

      // Loại bỏ tham chiếu tránh thay đổi mảng gốc
      tableColumns: employeeColumns.map((col) => ({ ...col })),

      // Các bản ghi đã được chọn (xoá nhiều)
      selectedRowsData: [],

      // Một hàng đang được xác nhận xoá
      activeRowState: null,

      // Cấu hình filter
      defaultFilterGroups: JSON.parse(JSON.stringify(employeeFilterGroups)),
      filterGroups: JSON.parse(JSON.stringify(employeeFilterGroups)),

      // Bộ lọc, tìm kiếm, sắp xếp, phân trang
      filterRequest: {
        page: 1,
        pageSize: 15,
        search: null,
        department: null,
        usageStatus: true,
        sortColumn: null,
        sortOrder: null,

        ...defaultFilter,
      },

      // Thông tin phân trang
      pagingInfo: {
        totalPages: 0,
        totalRecords: 0,
      },

      // Trạng thái của các popup
      isOpenTableCustomize: false,
      isOpenFilterPopup: false,

      // Trạng thái loading Excel
      isLoadingExportExcel: false,
    };
  },

  computed: {
    ...mapState("dialogStore", {
      dialogStore: (state) => state,
    }),

    /**
     * Description: Phát hiện bộ lọc được áp dụng
     * Author: txphuc (25/08/2023)
     */
    detectFilter() {
      const hasFilter = this.filterGroups?.some((filterGroup) => filterGroup.enabled);
      return hasFilter;
    },
  },

  watch: {
    /**
     * Description: Gọi lại data khi filter thay đổi
     * Author: txphuc (22/08/2023)
     */
    filterRequest: {
      handler: function () {
        this.getEmployeesData();
      },
      deep: true,
    },
  },

  methods: {
    /**
     * Description: Lấy các hàng được chọn
     * Author: txphuc (17/08/2023)
     */
    onSelectionChanged(data) {
      this.selectedRowsData = data.selectedRowsData;
    },

    /**
     * Description: Mở xem chi tiết nhân viên trong một cửa sổ mới
     * Author: txphuc (31/08/2023)
     */
    onClickNewWindow(row) {
      const employeeId = row.key;
      const routeData = this.$router.resolve({
        name: "employee-detail-view",
        params: { id: employeeId },
      });

      window.open(routeData.href, "_blank");
    },

    /**
     * Description: Mở form sửa bản ghi
     * Author: txphuc (24/08/2023)
     */
    onClickEditRow(row) {
      const employeeId = row.key;
      this.$router.push({ name: "employee-detail-update", params: { id: employeeId } });
    },

    /**
     * Description: Lưu bản ghi để xác nhận xoá
     * Author: txphuc (24/08/2023)
     */
    onClickDeleteRow(row) {
      this.activeRowState = row.data;

      this.showDeleteConfirmDialog(
        this.$t("employee.dialog.deleteEmployeeDesc", { employee: row?.data?.FullName })
      );
    },

    /**
     * Description: Xử lý khi double click vào một hàng
     * Author: txphuc (17/08/2023)
     */
    onRowDoubleClick(data) {
      this.$router.push({ name: "employee-detail-view", params: { id: data.key } });
    },

    /**
     * Description: Bỏ chọn tất cả dòng
     * Author: txphuc (17/08/2023)
     */
    clearAllSelection() {
      this.$refs.tableRef.clearAllSelection();
    },

    /**
     * Description: Lấy thông tin sắp xếp
     * Author: txphuc (17/08/2023)
     */
    onSortChange(column) {
      this.filterRequest.sortColumn = column.dataField;
      this.filterRequest.sortOrder = column.sortOrder;
    },

    /**
     * Description: Lấy thông tin ghim cột
     * Author: txphuc (17/08/2023)
     */
    onFixedColumnChange(columns) {
      this.tableColumns = columns;
    },

    /**
     * Description: Lưu sự thay đổi tuỳ chỉnh cột
     * Author: txphuc (18/08/2023)
     */
    applyTableCustomize(columns) {
      this.tableColumns = columns;
    },

    /**
     * Description: Xử lý sang trang tiếp theo
     * Author: txphuc (22/08/2023)
     */
    onNextPage(page) {
      this.filterRequest.page = page;
    },

    /**
     * Description: Xử lý sang trang trước đó
     * Author: txphuc (22/08/2023)
     */
    onPrevPage(page) {
      this.filterRequest.page = page;
    },

    /**
     * Description: Xử lý thay đổi page size
     * Author: txphuc (22/08/2023)
     */
    onPageSizeChange(pageSize) {
      this.filterRequest.pageSize = pageSize;

      // Về trang đầu khi đổi page size
      this.filterRequest.page = 1;
    },

    /**
     * Description: Apply search để bắt đầu tìm kiếm
     * Author: txphuc (22/08/2023)
     */
    applySearch(e) {
      this.filterRequest.search = e.event?.target?.value;

      // Về trang đầu khi tìm kiếm
      this.filterRequest.page = 1;
    },

    /**
     * Description: Áp dụng bộ lọc
     * Author: txphuc (25/08/2023)
     */
    applyFilter(newFilterGroups) {
      try {
        // Lưu bộ lọc
        this.filterGroups = newFilterGroups;

        // Xoá filter cũ
        const newRequest = {
          ...defaultFilter,
        };

        // Format lại filter để gọi API
        newFilterGroups.forEach((filterGroup) => {
          // Nếu filter group được bật
          if (filterGroup.enabled) {
            // Gán các trường filter
            filterGroup?.filters.forEach((filter) => {
              if (filter.key && filter.filterBy && filter.type) {
                if (filter.type == "date-between") {
                  // Khoảng ngày tháng (từ ngày - đến ngày)

                  newRequest[filter.key + "Start"] = formatDate(
                    new Date(filter.start),
                    "yyyy-MM-dd"
                  );

                  newRequest[filter.key + "End"] = formatDate(new Date(filter.end), "yyyy-MM-dd");

                  newRequest[filter.key + "FilterBy"] = filter.filterBy;
                } else {
                  // Các kiểu lọc khác chỉ có một giá trị
                  newRequest[filter.key] = filter.value;
                  newRequest[filter.key + "FilterBy"] = filter.filterBy;
                }
              }
            });
          }
        });

        this.filterRequest = {
          ...this.filterRequest,
          page: 1,
          ...newRequest,
        };
      } catch (error) {
        console.warn(error);
      }
    },

    /**
     * Description: Reset bộ lọc về mặc đình
     * Author: txphuc (25/08/2023)
     */
    resetFilter(defaultFilterGroups) {
      this.filterGroups = defaultFilterGroups;

      this.filterRequest = {
        ...this.filterRequest,
        page: 1,
        ...defaultFilter,
      };
    },

    /**
     * Description: Hàm load dữ liệu danh sách nhân viên từ api
     * Author: txphuc (27/06/2023)
     */
    async getEmployeesData() {
      try {
        this.$store.dispatch("commonStore/setLoading", true);

        const response = await employeeApi.filter(this.filterRequest);

        // Hiển thị dữ liệu ra bảng
        this.dataSource = response.data?.Data;

        // Lấy dữ liệu phân trang
        const totalRecords = response.data.TotalRecords;
        const pageSize = this.filterRequest.pageSize;
        const totalPages = Math.ceil(totalRecords / pageSize);

        this.pagingInfo.totalRecords = totalRecords;
        this.pagingInfo.totalPages = totalPages;

        this.$store.dispatch("commonStore/setLoading", false);
      } catch (error) {
        console.warn(error);
      }
    },

    /**
     * Description: Mở form thêm mới nhân viên
     * Author: txphuc (31/08/2023)
     */
    openFormForCreate() {
      this.$router.push({
        name: "employee-detail",
      });
    },

    /**
     * Description: Hiện dialog xác nhận xoá
     * Author: txphuc (24/06/2023).
     */
    showDeleteConfirmDialog(description) {
      this.$store.dispatch("dialogStore/showDeleteWarning", {
        title: this.$t("employee.dialog.deleteEmployeeTitle"),
        description,
        okHandler: this.handleDeleteEmployee,
        cancelHandler: this.hideConfirmDialog,
      });
    },

    /**
     * Description: Ẩn dialog xác nhận và bỏ hàng được chọn
     * Author: txphuc (27/06/2023).
     */
    hideConfirmDialog() {
      this.$store.dispatch("dialogStore/closeDialog");
      this.clearAllSelection();
      this.activeRowState = null;
    },

    /**
     * Description: Hàm xoá nhân viên active hoặc đang được chọn
     * Author: txphuc (11/07/2023)
     */
    async handleDeleteEmployee() {
      this.$store.dispatch("commonStore/setLoading", true);

      if (this.activeRowState) {
        // Nếu có hàng đang được active thì xoá hàng đó trước
        await this.deleteActiveEmployee();
      } else {
        // Xoá các hàng đang checked
        await this.deleteSelectedEmployee();
      }

      this.$store.dispatch("commonStore/setLoading", false);
    },

    /**
     * Description: Hàm xoá nhiều nhân viên đã được chọn
     * Author: txphuc (27/06/2023)
     */
    async deleteSelectedEmployee() {
      try {
        const deleteIds = this.selectedRowsData?.map((row) => row.EmployeeID);

        await employeeApi.deleteMultiple(deleteIds);

        // Chuyển về trang trước đó nếu xoá hét bản ghi ở trang hiện tại
        if (
          this.selectedRowsData.length == this.dataSource.length &&
          this.filterRequest.page == this.pagingInfo.totalPages
        ) {
          this.filterRequest.page = this.filterRequest.page - 1;
        } else {
          // Load lại data
          await this.getEmployeesData();
        }

        // Ẩn dialog xác nhận xoá và bỏ chọn tất cả
        this.hideConfirmDialog();

        // Hiện toast message xoá thành công
        this.$store.dispatch("toastStore/pushSuccessMessage", {
          message: this.$t("employee.toast.deleteSuccess"),
        });
      } catch (error) {
        console.warn(error);

        // Ẩn dialog xác nhận xoá
        this.hideConfirmDialog();

        // Load lại data
        await this.getEmployeesData();
      }
    },

    /**
     * Description: Hàm xoá một nhân viên đang active
     * Author: txphuc (11/07/2023)
     */
    async deleteActiveEmployee() {
      try {
        await employeeApi.delete(this.activeRowState.EmployeeID);

        // Ẩn dialog xác nhận xoá
        this.hideConfirmDialog();

        // Load lại data
        await this.getEmployeesData();

        // Hiện toast message xoá thành công
        this.$store.dispatch("toastStore/pushSuccessMessage", {
          message: this.$t("employee.toast.deleteSuccess"),
        });
      } catch (error) {
        console.warn(error);

        // Ẩn dialog xác nhận xoá
        this.hideConfirmDialog();

        // Load lại data
        await this.getEmployeesData();
      }
    },

    /**
     * Description: Download excel danh sách nhân viên
     * Author: txphuc (21/07/2023)
     */
    async downloadExcel(employeeIds = []) {
      try {
        this.isLoadingExportExcel = true;

        const response = await employeeApi.downloadExcel(excelExportSheets, employeeIds);

        const blob = new Blob([response.data], {
          type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        });

        const url = URL.createObjectURL(blob);

        const linkElement = document.createElement("a");
        linkElement.href = url;
        linkElement.download = "Xuat_khau_Danh_sach_nguoi_nop_thue.xlsx";
        linkElement.click();

        this.isLoadingExportExcel = false;
      } catch (error) {
        console.warn(error);
      }
    },

    /**
     * Description: Download Excel danh sách nhân viên theo ID
     * Author: txphuc (28/08/2023)
     */
    async exportListToExcel() {
      const employeeIds = this.selectedRowsData.map((row) => row.EmployeeID);

      await this.downloadExcel(employeeIds);
    },

    /**
     * Description: Xử lý sự kiện bàn phím
     * Author: txphuc (31/08/2023)
     */
    handleKeyboardEvent(e) {
      try {
        const keyCode = e.keyCode;
        const ctrlKey = e.ctrlKey;

        if (ctrlKey) {
          switch (keyCode) {
            case enums.key.NUM_1:
              e.preventDefault();

              // Ctrl + 1: Mở form thêm mới nhân viên
              this.openFormForCreate();

              break;

            case enums.key.D:
              // Ctrl + D: Xoá các nhân viên được chọn
              if (this.selectedRowsData.length > 0) {
                e.preventDefault();

                this.showDeleteConfirmDialog(
                  this.$t("employee.dialog.deleteMultipleEmployeeDesc", {
                    quantity: this.selectedRowsData.length,
                  })
                );
              }
              break;

            default:
              break;
          }
        }
      } catch (error) {
        console.warn(error);
      }
    },
  },

  /**
   * Description: Get dữ liệu khi khởi tạo
   * Author: txphuc (22/08/2023)
   */
  created: function () {
    this.getEmployeesData();
  },

  /**
   * Description: Gán sự kiện khi component được mounted
   * Author: txphuc (31/08/2023)
   */
  mounted: function () {
    window.addEventListener("keydown", this.handleKeyboardEvent);
  },

  /**
   * Description: Huỷ sự kiện khi component được destroyed
   * Author: txphuc (31/08/2023)
   */
  destroyed: function () {
    window.removeEventListener("keydown", this.handleKeyboardEvent);
  },
};
</script>

<style scoped></style>
