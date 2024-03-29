import enums from "@/enum/enum";
import i18n from "@/i18n";

const dialogStore = {
  namespaced: true,

  state: () => ({
    active: false,
    type: enums.dialog.type.INFO,
    title: i18n.t("dialog.warningTitle"),
    description: "",

    // Hàm xử lý đóng dialog
    cancelHandler: null,

    buttons: [],
  }),

  mutations: {
    /**
     * Description: Set trạng thái đóng/mở cho dialog
     * Author: txphuc (24/08/2023)
     */
    SET_ACTIVE(state, active) {
      state.active = active;
    },

    /**
     * Description: Set loại dialog
     * Author: txphuc (24/08/2023)
     */
    SET_TYPE(state, type) {
      state.type = type;
    },

    /**
     * Description: Set tiêu đề cho dialog
     * Author: txphuc (24/08/2023)
     */
    SET_TITLE(state, title) {
      state.title = title;
    },

    /**
     * Description: Set mô tả cho dialog
     * Author: txphuc (24/08/2023)
     */
    SET_DESCRIPTION(state, description) {
      state.description = description;
    },

    /**
     * Description: Set hàm xử lý đóng dialog
     * Author: txphuc (24/08/2023)
     */
    SET_CANCEL_HANDLER(state, handler) {
      state.cancelHandler = handler;
    },

    /**
     * Description: Set button hiển thị và hành động của nó
     * Author: txphuc (24/08/2023)
     */
    SET_BUTTONS(state, buttons) {
      state.buttons = buttons;
    },
  },
  actions: {
    /**
     * Description: Hiện cảnh báo
     * Author: txphuc: (03/08/2023)
     */
    showWarning({ commit }, description) {
      commit("SET_ACTIVE", true);
      commit("SET_TYPE", enums.dialog.type.WARNING);
      commit("SET_TITLE", i18n.t("dialog.warningTitle"));
      commit("SET_DESCRIPTION", description);

      commit("SET_BUTTONS", [
        { key: 1, text: i18n.t("button.ok"), color: "danger", focus: true, action: "closeDialog" },
      ]);
    },

    /**
     * Description: Hiện cảnh báo
     * Author: txphuc: (03/08/2023)
     */
    showExistFormWarning({ commit }, handler) {
      commit("SET_ACTIVE", true);
      commit("SET_TYPE", enums.dialog.type.WARNING);
      commit("SET_TITLE", i18n.t("dialog.closeFormWarningTitle"));
      commit("SET_DESCRIPTION", i18n.t("dialog.closeFormWarningDesc"));

      commit("SET_BUTTONS", [
        { key: 1, text: i18n.t("button.cancel"), color: "secondary", action: "closeDialog" },
        {
          key: 2,
          text: i18n.t("button.dontSave"),
          color: "solid-secondary",
          action: handler.cancel,
        },
        {
          key: 3,
          text: i18n.t("button.save"),
          color: "primary",
          focus: true,
          action: handler.submit,
        },
      ]);
    },

    /**
     * Description: Hiện cảnh báo xác nhận xoá
     * Author: txphuc: (03/08/2023)
     */
    showDeleteWarning(
      { commit },
      options = {
        title: "",
        description: "",
        okHandler: null,
        cancelHandler: null,
      }
    ) {
      commit("SET_ACTIVE", true);
      commit("SET_TYPE", enums.dialog.type.WARNING);
      commit("SET_TITLE", options.title);
      commit("SET_DESCRIPTION", options.description);

      commit("SET_CANCEL_HANDLER", options.cancelHandler);

      commit("SET_BUTTONS", [
        {
          key: 1,
          text: i18n.t("button.no"),
          color: "secondary",
          action: options.cancelHandler || "closeDialog",
        },
        {
          key: 2,
          text: i18n.t("button.yes"),
          color: "danger",
          focus: true,
          action: options.okHandler,
        },
      ]);
    },

    /**
     * Description: Đóng dialog
     * Author: txphuc: (03/08/2023)
     */
    closeDialog({ commit }) {
      commit("SET_ACTIVE", false);
      commit("SET_TYPE", enums.dialog.type.INFO);
      commit("SET_TITLE", "");
      commit("SET_DESCRIPTION", "");
      commit("SET_CANCEL_HANDLER", null);
      commit("SET_BUTTONS", []);
    },
  },
  getters: {},
};

export default dialogStore;
