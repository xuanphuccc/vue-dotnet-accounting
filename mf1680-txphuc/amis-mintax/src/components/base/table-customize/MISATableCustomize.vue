<template>
  <div @click.stop="" class="ms-table-customize">
    <div class="ms-table-customize__header">
      <div class="ms-table-customize__header-title">{{ $t("customizeColumn.title") }}</div>
      <div
        v-tooltip="$t('tooltip.close')"
        @click="$emit('close')"
        class="ms-table-customize__close"
      >
        <MISAIcon icon="times" />
      </div>
    </div>

    <div class="ms-table-customize__search">
      <MISATextBox v-model="search" :placeholder="$t('customizeColumn.search')">
        <MISAIcon size="20" icon="search" />
      </MISATextBox>
    </div>

    <div class="ms-table-customize__content">
      <!-- <MISACheckbox @click="toggleColumn(element?.key)" :checked="!element?.hide" /> -->
      <draggable v-model="localColumns">
        <transition-group>
          <li
            v-for="element in localColumns"
            :key="element.dataField"
            :class="['ms-table-customize__menu-item', { '--sticky': element?.fixed }]"
            v-show="element?.caption?.toLowerCase()?.includes(search.toLowerCase())"
          >
            <div class="ms-table-customize__menu-table-name-wrap">
              <DxCheckBox
                @value-changed="toggleColumn(element.dataField)"
                :value="element.visible !== false"
              />
              <div class="ms-table-customize__menu-table-name">{{ element?.caption }}</div>
            </div>
            <div class="ms-table-customize__menu-item-actions">
              <div class="ms-table-customize__item-action --pin-icon">
                <MISAIcon @click="pinColumn(element?.key)" size="20" icon="pin" />
              </div>
              <div class="ms-table-customize__item-action">
                <MISAIcon size="20" icon="draggable-dots" class="handle" />
              </div>
            </div>
          </li>
        </transition-group>
      </draggable>
    </div>

    <div class="ms-table-customize__footer">
      <MISAButton @click="resetDefault" color="secondary">{{
        $t("button.restoreDefault")
      }}</MISAButton>
      <MISAButton @click="saveChange" type="primary">{{ $t("button.save") }}</MISAButton>
    </div>
  </div>
</template>

<script>
import DxCheckBox from "devextreme-vue/check-box";
import MISAButton from "../button/MISAButton.vue";
import MISAIcon from "../icon/MISAIcon.vue";
import MISATextBox from "../text-box/MISATextBox.vue";
import draggable from "vuedraggable";

export default {
  name: "MISATableCustomize",
  emits: ["close", "change"],
  components: {
    MISAButton,
    MISAIcon,
    DxCheckBox,
    draggable,
    MISATextBox,
  },
  props: {
    // Mảng các cột của bảng
    columns: {
      type: Array,
      default() {
        return [];
      },
    },

    // Mảng các cột mặc định
    defaultColumns: {
      type: Array,
      default() {
        return [];
      },
    },
  },
  data: function () {
    return {
      // Deep clone prop sang state để thực hiện thay đổi (bắt buộc sử dụng v-if)
      // (tránh việc chưa bấm lưu nhưng đã thay đổi/không render lại)
      localColumns: this.columns.map((col) => ({ ...col })),

      // Tìm kiếm cột
      search: "",
    };
  },
  methods: {
    /**
     * Description: Lưu lại sự thay đổi
     * Author: txphuc (22/07/2023)
     */
    saveChange() {
      // Lấy dữ liệu đã được sắp xếp
      const separatedColumns = this.handleSeparateColumns();

      this.$emit("change", separatedColumns);

      // Đóng menu
      this.closeMenu();
    },

    /**
     * Description: Xử lý phân tách các cột được ghim
     * và các cột không được ghim
     * Author: txphuc (22/07/2023)
     */
    handleSeparateColumns() {
      try {
        let stickyColumns = [];
        let normalColumns = [];

        this.localColumns?.forEach((column) => {
          if (column.fixed) {
            stickyColumns = [...stickyColumns, { ...column }];
          } else {
            normalColumns = [...normalColumns, { ...column }];
          }
        });

        let result = [...stickyColumns, ...normalColumns];

        return result;
      } catch (error) {
        console.warn(error);
        return [];
      }
    },

    /**
     * Description: Xử lý ghim/bỏ ghim cột được chọn
     * Author: txphuc (22/07/2023)
     */
    pinColumn(dataField) {
      try {
        let column = this.localColumns.find((column) => column.dataField === dataField);

        if (column && column.fixed) {
          column.fixed = false;
        } else if (column) {
          column.fixed = true;
        }
      } catch (error) {
        console.warn(error);
      }
    },

    /**
     * Description: Xử lý ẩn/hiện cột được chọn
     * Author: txphuc (22/07/2023)
     */
    toggleColumn(dataField) {
      try {
        let column = this.localColumns.find((column) => column.dataField === dataField);

        if (column && (column.visible === true || column.visible === undefined)) {
          column.visible = false;
        } else if (column) {
          column.visible = true;
        }
      } catch (error) {
        console.warn(error);
      }
    },

    /**
     * Description: Xử lý reset về mặc định
     * Author: txphuc (22/07/2023)
     */
    resetDefault() {
      try {
        // Sao chép mảng loại bỏ tham chiếu
        // (tránh bị thay đổi mảng cột mặc định)
        this.localColumns = this.defaultColumns.map((col) => ({ ...col }));

        this.saveChange();
      } catch (error) {
        console.warn(error);
      }
    },

    /**
     * Description: Đóng menu khi click outside
     * Author: txphuc (22/07/2023)
     */
    closeMenu() {
      this.$emit("close");
    },
  },

  /**
   * Description: Thêm sự kiện click outside
   * Author: txphuc (22/07/2023)
   */
  mounted: function () {
    window.addEventListener("click", this.closeMenu);
  },

  /**
   * Description: Huỷ sự kiện click outside
   * Author: txphuc (22/07/2023)
   */
  destroyed: function () {
    window.removeEventListener("click", this.closeMenu);
  },
};
</script>

<style scoped>
@import url("./table-customize.scss");
</style>
