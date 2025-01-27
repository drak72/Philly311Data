<script setup lang="ts">
import { ref } from 'vue';

 defineProps<{
    placeholder: string
    disabled: boolean
 }>();

   const inputValue = ref('');
</script>

<template>
    <input type="text" :value="inputValue" @input="$emit('update:inputValue', ($event.target as HTMLInputElement).value)" :placeholder="placeholder" />
</template>

<style scoped>

</style>