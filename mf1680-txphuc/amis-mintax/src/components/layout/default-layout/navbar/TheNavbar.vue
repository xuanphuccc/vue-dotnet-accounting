<template>
  <header :class="['navbar', { '--compact': false }]">
    <div class="navbar__left">
      <div class="navbar__menu-icon" title="Menu"></div>
      <div class="navbar__logo-container">
        <div class="navbar__logo" title="Thuế TNCN"></div>
        <div class="navbar__logo-text">{{ $t("product") }}</div>
      </div>
    </div>
    <div class="navbar__right">
      <div class="navbar__title-wrap">
        <div class="navbar__company-avatar">M</div>

        <div class="navbar__title">
          <p>{{ $t("company") }}</p>

          <div class="navbar__title-toggle">
            <MISAIcon icon="angle-down" />
          </div>
        </div>
      </div>
      <div class="navbar__controls">
        <div v-tooltip="$t('tooltip.notification')" class="navbar__button">
          <MISAIcon icon="bell" />
        </div>
        <div v-tooltip="$t('tooltip.help')" class="navbar__button">
          <MISAIcon icon="question-circle" />
        </div>
        <div v-tooltip="$t('tooltip.otherFeatures')" class="navbar__button">
          <MISAIcon icon="menu-circle" />
        </div>

        <div @click.stop="toggleUserMenu" class="navbar__avatar-wrap">
          <img class="navbar__avatar" src="../../../../assets/img/avatar-default.png" alt="" />
        </div>
      </div>
    </div>

    <MISAUserMenu v-if="isOpenUserMenu" @click.stop="" />
  </header>
</template>

<script>
import MISAUserMenu from "@/components/base/user-menu/MISAUserMenu.vue";
import MISAIcon from "@/components/base/icon/MISAIcon.vue";

export default {
  name: "TheNavBar",
  components: {
    MISAIcon,
    MISAUserMenu,
  },
  data: function () {
    return {
      isOpenUserMenu: false,
    };
  },
  methods: {
    /**
     * Description: Hàm mở menu người dùng
     * Author: txphuc (11/07/2023)
     */
    toggleUserMenu: function () {
      this.isOpenUserMenu = !this.isOpenUserMenu;
    },

    /**
     * Description: Hàm đóng menu người dùng
     * Author: txphuc (11/07/2023)
     */
    closeUserMenu: function () {
      this.isOpenUserMenu = false;
    },
  },

  /**
   * Description: Xử lý sự kiện click ra ngoài thì tắt user menu
   * Author: txphuc (11/07/2023)
   */
  mounted: function () {
    window.addEventListener("click", this.closeUserMenu);
  },
};
</script>

<style lang="scss" scoped>
@import url("./navbar.scss");
</style>
