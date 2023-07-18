<template>
  <label @focus="openMenu" :tabindex="props.tabindex" class="ms-date-picker">
    <VueDatePicker
      :model-value="props.modelValue"
      @update:model-value="setDate"
      :format="formatInput"
      :text-input-options="textInputOptions"
      auto-apply
      keep-action-row
      :enable-time-picker="false"
      hide-input-icon
      :teleport="true"
      :position="props.position"
      space-confirm
      offset="4"
      placeholder="Chọn ngày"
      input-class-name="ms-date-picker__input"
      menu-class-name="ms-date-picker__menu"
      calendar-class-name="ms-date-picker__calendar"
      calendar-cell-class-name="ms-date-picker__calendar-cell"
      ref="datePickerRef"
    >
      <!-- menu header -->
      <template #month-year="{ month, year, handleMonthYearChange }">
        <div class="ms-date-picker__header">
          <div class="ms-date-picker__header-date">Tháng {{ month + 1 }}, {{ year }}</div>
          <div class="ms-date-picker__header-controls">
            <MISAIcon @click="handleMonthYearChange(false)" icon="angle-left" />
            <MISAIcon @click="handleMonthYearChange(true)" icon="angle-right" />
          </div>
        </div>
      </template>

      <!-- calendar header -->
      <template #calendar-header="{ day }">
        <div class="ms-date-picker__calendar-header-item">{{ day }}</div>
      </template>

      <template #action-row>
        <div @click="getToday" class="ms-date-picker__action">Hôm nay</div>
      </template>
    </VueDatePicker>
    <MISAIcon icon="date" />
  </label>
</template>

<script setup>
import VueDatePicker from "@vuepic/vue-datepicker";
import MISAIcon from "../icon/MISAIcon.vue";
import formatDate from "../../../helper/format-date";
import { ref } from "vue";

const emit = defineEmits(["update:modelValue"]);
const datePickerRef = ref(null);

const props = defineProps({
  // Giá trị
  modelValue: Date,

  // Tabindex
  tabindex: [String, Number],

  // Vị trí của menu
  position: {
    type: String,
    default: "center",
  },
});

const textInputOptions = ref({
  enterSubmit: true,
  tabSubmit: true,
  openMenu: true,
});

/**
 * Description: Update giá trị được chọn
 * Author: txphuc (17/07/2023)
 */
const setDate = (value) => {
  emit("update:modelValue", value);
};

/**
 * Description: Update giá trị là ngày hôm nay
 * Author: txphuc (17/07/2023)
 */
const getToday = () => {
  try {
    const today = new Date();

    emit("update:modelValue", today);

    // Đóng menu chọn ngày
    if (datePickerRef.value) {
      datePickerRef.value.closeMenu();
    }
  } catch (error) {
    console.warn(error);
  }
};

/**
 * Description: Format date hiển thị lên input
 * Author: txphuc (17/07/2023)
 */
const formatInput = (date) => {
  try {
    if (typeof date === "object") {
      return formatDate(date.toJSON(), "DD/MM/YYYY");
    } else {
      return null;
    }
  } catch (error) {
    console.warn(error);
  }
};

const openMenu = () => {
  datePickerRef.value.openMenu();
};
</script>

<style>
@import url("@vuepic/vue-datepicker/dist/main.css");
@import url("./date-picker.css");
</style>